using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;

namespace Happer.TestHttpClient
{
    class Program
    {
        private static ILog _log;
        private static HttpClient _httpClient;
        private static Encoding _textEncoding = new UTF8Encoding(false); // UTF8 NoBOM

        static Program()
        {
            NLogLogger.Use();
            _log = Logger.Get<Program>();
            _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
        }

        static void Main(string[] args)
        {
            var time = GetString(@"http://localhost:3202/time");
            _log.DebugFormat("{0}", time);

            var json = GetString(@"http://localhost:3202/json");
            _log.DebugFormat("{0}", json);

            var xml = GetString(@"http://localhost:3202/xml");
            _log.DebugFormat("{0}", xml);

            Console.ReadKey();
        }

        public static string GetString(string requestUri)
        {
            HttpStatusCode throwAway;
            return GetString(requestUri, out throwAway);
        }

        public static string GetString(string requestUri, out HttpStatusCode statusCode)
        {
            string result = string.Empty;
            statusCode = HttpStatusCode.InternalServerError;

            try
            {
                var response = _httpClient.GetAsync(requestUri).Result;
                statusCode = response.StatusCode;

                byte[] responseBody = null;
                if (response.IsSuccessStatusCode)
                {
                    responseBody = response.Content.ReadAsByteArrayAsync().Result;
                }
                else
                {
                    _log.WarnFormat("GetString, Uri[{0}], StatusCode[{1}|{2}].",
                        requestUri, (int)response.StatusCode, response.StatusCode.ToString());

                    throw new InvalidOperationException(
                        string.Format("HTTP [{0}] request to Uri[{1}] responses with StatusCode[{2}|{3}] is unanticipated.",
                            response.RequestMessage.Method, response.RequestMessage.RequestUri,
                            (int)response.StatusCode, response.StatusCode.ToString()));
                }

                if (responseBody != null && responseBody.Length > 0)
                {
                    result = _textEncoding.GetString(responseBody, 0, responseBody.Length);
                }
            }
            catch (Exception ex)
            {
                _log.Error(string.Format("GetString, Uri[{0}], Error[{1}].", requestUri, ex.Message), ex);
            }

            return result;
        }
    }
}
