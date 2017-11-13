using Happer.Http;

namespace Happer.TestHapperServer
{
    public class PlainModule : Module
    {
        public PlainModule()
        {
            Get("/plaintext", _ => "Hello, World!");
        }
    }
}
