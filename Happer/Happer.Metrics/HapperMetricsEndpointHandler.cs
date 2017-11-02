using System.Collections.Generic;
using System.Linq;
using Happer.Http;
using Metrics.Endpoints;

namespace Happer.Metrics
{
    internal class HapperMetricsEndpointHandler : AbstractMetricsEndpointHandler<Request>
    {
        public HapperMetricsEndpointHandler(IEnumerable<MetricsEndpoint> endpoints)
            : base(endpoints)
        { }

        protected override MetricsEndpointRequest CreateRequest(Request requestInfo)
        {
            var headers = requestInfo.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray());
            return new MetricsEndpointRequest(headers);
        }
    }
}
