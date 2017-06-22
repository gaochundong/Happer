using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Happer.Http;
using Logrila.Logging;

namespace Happer.Hosting.Self
{
    public class SelfHost
    {
        private ILog _log = Logger.Get<SelfHost>();
        private IEngine _engine;
        private IList<Uri> _baseUriList;
        private HttpListener _listener;
        private CancellationTokenSource _keepProcessSource = null;
        private IRateLimiter _rateLimiter = null;

        public SelfHost(IEngine engine, params Uri[] baseUris)
            : this(engine, NoneRateLimiter.None, baseUris)
        {
        }

        public SelfHost(IEngine engine, int maxConcurrentNumber, params Uri[] baseUris)
            : this(engine, new CountableRateLimiter(maxConcurrentNumber), baseUris)
        {
        }

        public SelfHost(IEngine engine, IRateLimiter rateLimiter, params Uri[] baseUris)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (rateLimiter == null)
                throw new ArgumentNullException("rateLimiter");
            if (baseUris == null || baseUris.Length == 0)
                throw new ArgumentNullException("baseUris");

            _engine = engine;
            _rateLimiter = rateLimiter;
            _baseUriList = baseUris;
        }

        public bool IsListening { get { return _listener != null ? _listener.IsListening : false; } }

        public void Start()
        {
            StartListener();

            _keepProcessSource = new CancellationTokenSource();

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                try
                {
                    var cancellationToken = _keepProcessSource.Token;
                    cancellationToken.ThrowIfCancellationRequested();

                    _listener.GetContextAsync().ContinueWith(HandleContext, _keepProcessSource, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // When stopping, Wait will throw 'The operation was canceled.'
                }
                catch (InvalidOperationException)
                {
                    // When stopping, GetContextAsync will throw 'Please call the Start() method before calling this method.'
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                }
            }
        }

        public void Stop()
        {
            if (_keepProcessSource != null)
            {
                _keepProcessSource.Cancel();
            }
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
            }
        }

        private void HandleContext(Task<HttpListenerContext> context, object state)
        {
            try
            {
                var cancellationToken = ((CancellationTokenSource)state).Token;
                cancellationToken.ThrowIfCancellationRequested();

                if (!context.IsCompleted)
                {
                    context.Wait(cancellationToken);
                }

                _rateLimiter.Wait(cancellationToken);
                try
                {
                    _listener.GetContextAsync().ContinueWith(HandleContext, state, cancellationToken);
                    Process(context.Result, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    _rateLimiter.Release();
                }
            }
            catch (OperationCanceledException)
            {
                // When stopping, Wait will throw 'The operation was canceled.'
            }
            catch (InvalidOperationException)
            {
                // When stopping, GetContextAsync will throw 'Please call the Start() method before calling this method.'
            }
            catch (HttpListenerException)
            {
                // When stopping, BeginGetContext will throw 'Incorrect function'
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        protected virtual async Task Process(HttpListenerContext httpContext, CancellationToken cancellationToken)
        {
            try
            {
                if (httpContext.Request.IsWebSocketRequest)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    httpContext.Response.Close();
                }
                else
                {
                    var baseUri = GetBaseUri(httpContext.Request.Url);
                    if (baseUri == null)
                        throw new InvalidOperationException(string.Format(
                            "Unable to locate base URI for request: {0}", httpContext.Request.Url));
                    var context = await _engine.HandleHttp(httpContext, baseUri, cancellationToken).ConfigureAwait(false);
                    context.Dispose();
                }
            }
            catch (NotSupportedException)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                httpContext.Response.Close();
            }
            catch (InvalidOperationException)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                httpContext.Response.Close();
            }
            catch (Exception)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.Close();
            }
        }

        private void StartListener()
        {
            if (TryStartListener())
            {
                return;
            }

            if (!TryAddUrlReservations())
            {
                throw new InvalidOperationException("Unable to configure namespace reservation.");
            }

            if (!TryStartListener())
            {
                throw new InvalidOperationException("Unable to start listener.");
            }
        }

        private bool TryStartListener()
        {
            try
            {
                // if the listener fails to start, it gets disposed;
                // so we need a new one, each time.
                _listener = new HttpListener();
                foreach (var prefix in GetPrefixes())
                {
                    _listener.Prefixes.Add(prefix);
                }

                _listener.Start();

                return true;
            }
            catch (HttpListenerException e)
            {
                int ACCESS_DENIED = 5;
                if (e.ErrorCode == ACCESS_DENIED)
                {
                    return false;
                }

                throw;
            }
        }

        private bool TryAddUrlReservations()
        {
            var user = WindowsIdentity.GetCurrent().Name;

            foreach (var prefix in GetPrefixes())
            {
                // netsh http add urlacl url=http://+:222222/test
                // netsh http add urlacl url=http://+:222222/test user=domain\user
                if (!NetSh.AddUrlAcl(prefix, user))
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerable<string> GetPrefixes()
        {
            foreach (var baseUri in _baseUriList)
            {
                var prefix = new UriBuilder(baseUri).ToString();

                bool rewriteLocalhost = true;
                if (rewriteLocalhost && !baseUri.Host.Contains("."))
                {
                    prefix = prefix.Replace("localhost", "+");
                }

                yield return prefix;
            }
        }

        protected Uri GetBaseUri(Uri requestUri)
        {
            var result = _baseUriList.FirstOrDefault(uri => uri.IsCaseInsensitiveBaseOf(requestUri));

            if (result != null)
            {
                return result;
            }

            return new Uri(requestUri.GetLeftPart(UriPartial.Authority));
        }
    }
}
