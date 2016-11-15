using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Happer.Http;

namespace Happer.Hosting.Self
{
    public class SelfHost
    {
        private IEngine _engine;
        private IList<Uri> _baseUriList;
        private HttpListener _listener;
        private bool _keepProcessing = false;
        private IRateLimiter _rateLimiter = new CountableRateLimiter(Environment.ProcessorCount * 2);

        public SelfHost(IEngine engine, params Uri[] baseUris)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (baseUris == null || baseUris.Length == 0)
                throw new ArgumentNullException("baseUris");

            _engine = engine;
            _baseUriList = baseUris;
        }

        public bool IsListening { get { return _listener != null ? _listener.IsListening : false; } }

        public void Start()
        {
            StartListener();

            _keepProcessing = true;

            // Launch a main thread that will listen for requests and then process them.
            Task.Factory.StartNew(async () =>
            {
                await StartProcess();
            })
            .ConfigureAwait(false);
        }

        public void Stop()
        {
            if (_listener != null && _listener.IsListening)
            {
                _keepProcessing = false;
                _listener.Stop();
            }
        }

        private async Task StartProcess()
        {
            // A main thread keep accepting the requests.
            while (_keepProcessing)
            {
                await _rateLimiter.WaitAsync();

                var context = await _listener.GetContextAsync();

                // Launch a child thread to handle the request.
                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        await Process(context).ConfigureAwait(false);
                    }
                    finally
                    {
                        _rateLimiter.Release();
                    }
                })
                .Forget();
            }
        }

        protected virtual async Task Process(HttpListenerContext httpContext)
        {
            try
            {
                var cancellationToken = new CancellationToken();

                // Each request is processed in its own execution thread.
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
            catch (Exception)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
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
