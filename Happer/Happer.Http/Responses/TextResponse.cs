using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Happer.Http.Responses
{
    public class TextResponse : Response
    {
        private const string TextPlainContentType = "text/plain";
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false); // UTF8 NoBOM

        public TextResponse(
            string contents,
            string contentType = null)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = TextPlainContentType;
            }

            this.ContentType = GetContentType(contentType, _defaultEncoding);
            this.StatusCode = HttpStatusCode.OK;

            if (contents != null)
            {
                this.Contents = stream =>
                {
                    var data = _defaultEncoding.GetBytes(contents);
                    stream.Write(data, 0, data.Length);
                };
            }
        }

        public TextResponse(
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string contents = null,
            string contentType = null,
            Encoding encoding = null,
            IDictionary<string, string> headers = null,
            IEnumerable<Cookie> cookies = null)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = TextPlainContentType;
            }

            if (encoding == null)
            {
                encoding = _defaultEncoding;
            }

            this.ContentType = GetContentType(contentType, encoding);
            this.StatusCode = statusCode;

            if (contents != null)
            {
                this.Contents = stream =>
                {
                    var data = encoding.GetBytes(contents);
                    stream.Write(data, 0, data.Length);
                };
            }

            if (headers != null)
            {
                this.Headers = headers;
            }

            if (cookies != null)
            {
                foreach (var cookie in cookies)
                {
                    this.Cookies.Add(cookie);
                }
            }
        }

        private static string GetContentType(string contentType, Encoding encoding)
        {
            return !contentType.Contains("charset")
                ? string.Concat(contentType, "; charset=", encoding.WebName)
                : contentType;
        }
    }
}
