using Happer.Http;

namespace Happer.TestHttpServer
{
    public class PerformanceModule : Module
    {
        public PerformanceModule()
        {
            Get("/performance", x => "Hello, World!");
        }
    }
}
