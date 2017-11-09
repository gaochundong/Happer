using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Happer.Http;

namespace Happer
{
    public static class ResponseCompressionExtensions
    {
        public static void EnableResponseCompression(this IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(ProcessCompression);
        }

        public static void ProcessCompression(Context context)
        {
            if (context.Response.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            if (!RequestIsGzipCompatible(context.Request))
            {
                return;
            }

            if (!ResponseIsMimeTypeCompatible(context.Response))
            {
                return;
            }

            CompressResponseGzipContentEncoding(context.Response);
        }

        private static List<string> ValidMimes =
            new List<string>
            {
                "text/css",
                "text/html",
                "text/plain",
                "application/xml",
                "application/json",
                "application/xaml+xml",
                "application/x-javascript"
            };

        private static bool RequestIsGzipCompatible(Request request)
        {
            return request.Headers.AcceptEncoding.Any(x => x.Contains("gzip"));
        }

        private static bool ResponseIsMimeTypeCompatible(Response response)
        {
            return ValidMimes.Any(x => x == response.ContentType);
        }

        private static void CompressResponseGzipContentEncoding(Response response)
        {
            response.WithHeader("Content-Encoding", "gzip");

            var contents = response.Contents;
            response.Contents =
                responseStream =>
                {
                    using (var gzip = new GZipStream(responseStream, CompressionMode.Compress))
                    {
                        contents(gzip);
                    }
                };
        }
    }
}
