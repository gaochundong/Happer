using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Happer.Http.Serialization;

namespace Happer.Serialization
{
    public class XmlSerializer : ISerializer
    {
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false); // UTF8 NoBOM

        public XmlSerializer()
        {
        }

        public bool CanSerialize(string contentType)
        {
            return IsXmlType(contentType);
        }

        public IEnumerable<string> Extensions
        {
            get { yield return "xml"; }
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            using (var writer = new StreamWriter(new UnclosableStreamWrapper(outputStream), _defaultEncoding))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TModel));
                serializer.Serialize(writer, model);
            }
        }

        private static bool IsXmlType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }

            var contentMimeType = contentType.Split(';')[0];

            return contentMimeType.Equals("application/xml", StringComparison.OrdinalIgnoreCase)
                || contentMimeType.Equals("text/xml", StringComparison.OrdinalIgnoreCase)
                || contentMimeType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase);
        }
    }
}
