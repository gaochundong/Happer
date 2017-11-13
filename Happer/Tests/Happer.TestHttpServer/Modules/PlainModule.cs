using Happer.Http;

namespace Happer.TestHttpServer
{
    public class PlainModule : Module
    {
        public PlainModule()
        {
            Get("/plaintext", _ => "Hello, World!");
        }
    }
}
