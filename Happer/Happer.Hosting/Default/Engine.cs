using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Happer.Http;
using Happer.Http.Utilities;
using Happer.StaticContent;

namespace Happer
{
    public class Engine : IEngine
    {
        private StaticContentProvider _staticContentProvider;
        private RequestDispatcher _requestDispatcher;

        public Engine(StaticContentProvider staticContentProvider, RequestDispatcher requestDispatcher)
        {
            if (staticContentProvider == null)
                throw new ArgumentNullException("staticContentProvider");
            if (requestDispatcher == null)
                throw new ArgumentNullException("requestDispatcher");

            _staticContentProvider = staticContentProvider;
            _requestDispatcher = requestDispatcher;
        }

        public async Task<Context> HandleHttp(HttpListenerContext httpContext, Uri baseUri, CancellationToken cancellationToken)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            var context = new Context() { Request = ConvertRequest(baseUri, httpContext.Request) };

            var staticContentResponse = _staticContentProvider.GetContent(context);
            if (staticContentResponse != null)
            {
                context.Response = staticContentResponse;
                return context;
            }

            var pipelines = new Pipelines();

            await InvokeRequestLifeCycle(context, cancellationToken, pipelines).ConfigureAwait(false);

            ConvertResponse(context.Response, httpContext.Response);

            return context;
        }

        private Request ConvertRequest(Uri baseUri, HttpListenerRequest httpRequest)
        {
            var expectedRequestLength = GetExpectedRequestLength(ConvertToDictionary(httpRequest.Headers));

            var url = new Url
            {
                Scheme = httpRequest.Url.Scheme,
                HostName = httpRequest.Url.Host,
                Port = httpRequest.Url.IsDefaultPort ? null : (int?)httpRequest.Url.Port,
                BasePath = baseUri.AbsolutePath.TrimEnd('/'),
                Path = baseUri.MakeAppLocalPath(httpRequest.Url),
                Query = httpRequest.Url.Query,
            };

            var fieldCount = httpRequest.ProtocolVersion.Major == 2 ? 1 : 2;
            var protocolVersion = string.Format("HTTP/{0}", httpRequest.ProtocolVersion.ToString(fieldCount));

            byte[] certificate = null;
            if (httpRequest.IsSecureConnection)
            {
                var x509Certificate = httpRequest.GetClientCertificate();
                if (x509Certificate != null)
                {
                    certificate = x509Certificate.RawData;
                }
            }

            return new Request(
                httpRequest.HttpMethod,
                url,
                RequestStream.FromStream(httpRequest.InputStream, expectedRequestLength, false),
                ConvertToDictionary(httpRequest.Headers),
                (httpRequest.RemoteEndPoint != null) ? httpRequest.RemoteEndPoint.Address.ToString() : null,
                protocolVersion,
                certificate);
        }

        private void ConvertResponse(Response response, HttpListenerResponse httpResponse)
        {
            foreach (var header in response.Headers)
            {
                if (!IgnoredHeaders.IsIgnored(header.Key))
                {
                    httpResponse.AddHeader(header.Key, header.Value);
                }
            }

            foreach (var cookie in response.Cookies)
            {
                httpResponse.Headers.Add(HttpResponseHeader.SetCookie, cookie.ToString());
            }

            if (response.ReasonPhrase != null)
            {
                httpResponse.StatusDescription = response.ReasonPhrase;
            }

            if (response.ContentType != null)
            {
                httpResponse.ContentType = response.ContentType;
            }

            httpResponse.StatusCode = (int)response.StatusCode;

            OutputWithContentLength(response, httpResponse);
        }

        private async Task<Context> InvokeRequestLifeCycle(Context context, CancellationToken cancellationToken, IPipelines pipelines)
        {
            try
            {
                var response = await InvokePreRequestHook(context, cancellationToken, pipelines.BeforeRequest).ConfigureAwait(false);

                if (response == null)
                {
                    response = await _requestDispatcher.Dispatch(context, cancellationToken).ConfigureAwait(false);
                }

                context.Response = response;

                await InvokePostRequestHook(context, cancellationToken, pipelines.AfterRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                InvokeOnErrorHook(context, pipelines.OnError, ex);
            }

            return context;
        }

        private static Task<Response> InvokePreRequestHook(Context context, CancellationToken cancellationToken, BeforePipeline pipeline)
        {
            return pipeline == null ? Task.FromResult<Response>(null) : pipeline.Invoke(context, cancellationToken);
        }

        private static Task InvokePostRequestHook(Context context, CancellationToken cancellationToken, AfterPipeline pipeline)
        {
            return pipeline == null ? Task.FromResult<object>(null) : pipeline.Invoke(context, cancellationToken);
        }

        private static void InvokeOnErrorHook(Context context, ErrorPipeline pipeline, Exception errorException)
        {
            try
            {
                if (pipeline == null)
                {
                    throw new RequestPipelinesException(errorException);
                }

                var onErrorResult = pipeline.Invoke(context, errorException);

                if (onErrorResult == null)
                {
                    throw new RequestPipelinesException(errorException);
                }

                context.Response = new Response { StatusCode = HttpStatusCode.InternalServerError };
            }
            catch (Exception)
            {
                context.Response = new Response { StatusCode = HttpStatusCode.InternalServerError };
            }
        }

        private static void OutputWithContentLength(Response response, HttpListenerResponse httpResponse)
        {
            byte[] buffer;
            using (var memoryStream = new MemoryStream())
            {
                response.Contents.Invoke(memoryStream);
                buffer = memoryStream.ToArray();
            }

            var contentLength = (response.Headers.ContainsKey("Content-Length")) ?
                Convert.ToInt64(response.Headers["Content-Length"]) :
                buffer.Length;

            httpResponse.SendChunked = false;
            httpResponse.ContentLength64 = contentLength;

            using (var output = httpResponse.OutputStream)
            {
                using (var writer = new BinaryWriter(output))
                {
                    writer.Write(buffer);
                    writer.Flush();
                }
            }
        }

        private static long GetExpectedRequestLength(IDictionary<string, IEnumerable<string>> incomingHeaders)
        {
            if (incomingHeaders == null)
            {
                return 0;
            }

            if (!incomingHeaders.ContainsKey("Content-Length"))
            {
                return 0;
            }

            var headerValue = incomingHeaders["Content-Length"].SingleOrDefault();

            if (headerValue == null)
            {
                return 0;
            }

            long contentLength;

            return !long.TryParse(headerValue, NumberStyles.Any, CultureInfo.InvariantCulture, out contentLength) ?
                0 : contentLength;
        }

        private static IDictionary<string, IEnumerable<string>> ConvertToDictionary(NameValueCollection source)
        {
            return source.AllKeys.ToDictionary<string, string, IEnumerable<string>>(key => key, source.GetValues);
        }

        private static class IgnoredHeaders
        {
            private static readonly HashSet<string> _knownHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "content-length",
                "content-type",
                "transfer-encoding",
                "keep-alive"
            };

            public static bool IsIgnored(string headerName)
            {
                return _knownHeaders.Contains(headerName);
            }
        }
    }
}
