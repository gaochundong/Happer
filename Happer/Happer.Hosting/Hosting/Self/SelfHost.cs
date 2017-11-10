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
        private static readonly int DefaultConcurrentAccepts = 3 * Environment.ProcessorCount;

        public SelfHost(IEngine engine, params Uri[] baseUris)
            : this(engine, DefaultConcurrentAccepts, baseUris)
        {
        }

        public SelfHost(IEngine engine, int concurrentAccepts, params Uri[] baseUris)
            : this(engine, new CountableRateLimiter(concurrentAccepts), baseUris)
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

            var cancellationToken = _keepProcessSource.Token;
            cancellationToken.ThrowIfCancellationRequested();

            int concurrentAccepts = _rateLimiter.CurrentCount;
            for (int i = 0; i < concurrentAccepts; i++)
            {
                try
                {
                    _listener.GetContextAsync().ContinueWith(HandleListenerContext, _keepProcessSource, cancellationToken).Forget();
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
        }

        public void Stop()
        {
            var keepProcessSource = _keepProcessSource;
            var listener = _listener;

            if (keepProcessSource != null)
            {
                keepProcessSource.Cancel();
            }
            if (listener != null && listener.IsListening)
            {
                listener.Stop();
            }
        }

        private async Task HandleListenerContext(Task<HttpListenerContext> listenerContext, object state)
        {
            try
            {
                var cancellationToken = ((CancellationTokenSource)state).Token;
                cancellationToken.ThrowIfCancellationRequested();

                _rateLimiter.Wait(cancellationToken);
                try
                {
                    await Process(listenerContext.Result, cancellationToken);
                }
                finally
                {
                    _rateLimiter.Release();
                    _listener.GetContextAsync().ContinueWith(HandleListenerContext, state, cancellationToken).Forget();
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

        protected virtual async Task Process(HttpListenerContext listenerContext, CancellationToken cancellationToken)
        {
            try
            {
                var baseUri = GetBaseUri(listenerContext.Request.Url);
                if (baseUri == null)
                    throw new InvalidOperationException(string.Format(
                        "Unable to locate base URI for request: {0}", listenerContext.Request.Url));
                var context = await _engine.HandleHttp(listenerContext, baseUri, cancellationToken).ConfigureAwait(false);
                context.Dispose();
            }
            catch (NotSupportedException)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                listenerContext.Response.Close();
            }
            catch (InvalidOperationException)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                listenerContext.Response.Close();
            }
            catch (Exception)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                listenerContext.Response.Close();
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
            var urlReservations = new UrlReservations();
            var user = !string.IsNullOrWhiteSpace(urlReservations.User)
                ? urlReservations.User
                : WindowsIdentity.GetCurrent().Name;

            foreach (var prefix in GetPrefixes())
            {
                // https://msdn.microsoft.com/en-us/library/windows/desktop/cc307223(v=vs.85).aspx
                // Reserves the specified URL for non-administrator users and accounts. 
                // The discretionary access control list (DACL) can be specified by using an account name 
                // with the listen and delegate parameters or by using a security descriptor definition language (SDDL) string.
                // netsh http add urlacl url=http://+:3202/MyUri
                // netsh http add urlacl url=http://+:3202/MyUri user=DOMAIN\user
                // netsh http delete urlacl url=http://+:3202/MyUri
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
