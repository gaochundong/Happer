using System;
using System.IO;
using System.Net;
using System.Text;
using Happer.Http.Serialization;

namespace Happer.Http.Responses
{
    public class JsonResponse<TModel> : Response
    {
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false); // UTF8 NoBOM

        public JsonResponse(TModel model, ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new InvalidOperationException("JSON Serializer not set");
            }

            this.Contents = model == null ? NoBody : GetJsonContents(model, serializer);
            this.ContentType = DefaultContentType;
            this.StatusCode = HttpStatusCode.OK;
        }

        private static string DefaultContentType
        {
            get { return string.Concat("application/json", Encoding); }
        }

        private static string Encoding
        {
            get { return string.Concat("; charset=", _defaultEncoding.WebName); }
        }

        private static Action<Stream> GetJsonContents(TModel model, ISerializer serializer)
        {
            return stream => serializer.Serialize(DefaultContentType, model, stream);
        }
    }

    public class JsonResponse : JsonResponse<object>
    {
        public JsonResponse(object model, ISerializer serializer) : base(model, serializer)
        {
        }
    }
}
