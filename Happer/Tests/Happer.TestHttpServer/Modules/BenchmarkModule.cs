using Happer.Http;

namespace Happer.TestHttpServer
{
    public class BenchmarkModule : Module
    {
        public BenchmarkModule()
        {
            Get("/plaintext", x => "Hello, World!");
        }
    }
}
