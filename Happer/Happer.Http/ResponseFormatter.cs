using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Happer.Http.Responses;
using Happer.Http.Serialization;

namespace Happer.Http
{
    public class ResponseFormatter
    {
        private readonly Context _context;
        private readonly IEnumerable<ISerializer> _serializers;
        private static ISerializer _jsonSerializer;
        private static ISerializer _xmlSerializer;

        public ResponseFormatter(Context context, IEnumerable<ISerializer> serializers)
        {
            _context = context;
            _serializers = serializers.ToArray();
        }

        public IEnumerable<ISerializer> Serializers
        {
            get { return _serializers; }
        }

        public Context Context
        {
            get { return _context; }
        }

        public Response FromStream(Stream stream, string contentType)
        {
            return new StreamResponse(() => stream, contentType);
        }

        public Response FromStream(Func<Stream> streamDelegate, string contentType)
        {
            return new StreamResponse(streamDelegate, contentType);
        }

        public Response AsText(string contents, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new TextResponse(statusCode: statusCode, contents: contents);
        }

        public Response AsText(string contents, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new TextResponse(statusCode: statusCode, contents: contents, contentType: contentType);
        }

        public Response AsImage(string applicationRelativeFilePath)
        {
            return this.AsFile(applicationRelativeFilePath);
        }

        public Response AsFile(string applicationRelativeFilePath, string contentType)
        {
            return new GenericFileResponse(applicationRelativeFilePath, contentType, this.Context);
        }

        public Response AsFile(string applicationRelativeFilePath)
        {
            return new GenericFileResponse(applicationRelativeFilePath, this.Context);
        }

        public Response AsJson<TModel>(TModel model, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var serializer = _jsonSerializer ?? (_jsonSerializer = this.Serializers.FirstOrDefault(s => s.CanSerialize("application/json")));

            var response = new JsonResponse<TModel>(model, serializer);
            response.StatusCode = statusCode;

            return response;
        }

        public Response AsXml<TModel>(TModel model, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var serializer = _xmlSerializer ?? (_xmlSerializer = this.Serializers.FirstOrDefault(s => s.CanSerialize("application/xml")));

            var response = new XmlResponse<TModel>(model, serializer);
            response.StatusCode = statusCode;

            return response;
        }

        public Response AsRedirect(string location, RedirectResponse.RedirectType type = RedirectResponse.RedirectType.SeeOther)
        {
            return new RedirectResponse(this.Context.ToFullPath(location), type);
        }

        public Response AsHtml(string html, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new HtmlResponse(statusCode,
                stream =>
                {
                    var writer = new StreamWriter(stream);
                    writer.Write(html);
                    writer.Flush();
                });
        }
    }
}
