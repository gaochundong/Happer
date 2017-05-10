using System;
using System.IO;
using System.Net;
using System.Text;
using Happer.Http.Serialization;

namespace Happer.Http.Responses
{
    public class XmlResponse<TModel> : Response
    {
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false); // UTF8 NoBOM

        public XmlResponse(TModel model, ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new InvalidOperationException("XML Serializer not set");
            }

            this.Contents = GetXmlContents(model, serializer);
            this.ContentType = DefaultContentType;
            this.StatusCode = HttpStatusCode.OK;
        }

        private static string DefaultContentType
        {
            get { return string.Concat("application/xml", Encoding); }
        }

        private static string Encoding
        {
            get { return string.Concat("; charset=", _defaultEncoding.WebName); }
        }

        private static Action<Stream> GetXmlContents(TModel model, ISerializer serializer)
        {
            return stream => serializer.Serialize(DefaultContentType, model, stream);
        }
    }
}
