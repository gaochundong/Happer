using System.Collections.Generic;
using System.Linq;
using Happer.Http.Serialization;

namespace Happer.Http
{
    public class ResponseFormatterFactory
    {
        private readonly IEnumerable<ISerializer> serializers;

        public ResponseFormatterFactory(IEnumerable<ISerializer> serializers)
        {
            this.serializers = serializers.ToArray();
        }

        public ResponseFormatter Create(Context context)
        {
            return new ResponseFormatter(context, this.serializers);
        }
    }
}
