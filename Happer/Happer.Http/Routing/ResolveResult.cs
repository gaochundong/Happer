using System;

namespace Happer.Http.Routing
{
    public class ResolveResult
    {
        public ResolveResult()
        {
        }

        public ResolveResult(Route route, DynamicDictionary parameters)
        {
            this.Route = route;
            this.Parameters = parameters;
        }

        public Route Route { get; set; }
        public DynamicDictionary Parameters { get; set; }

        public BeforePipeline Before { get; set; }
        public AfterPipeline After { get; set; }
        public Func<Context, Exception, dynamic> OnError { get; set; }
    }
}
