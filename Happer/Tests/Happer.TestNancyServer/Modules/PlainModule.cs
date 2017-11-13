using Nancy;

namespace Happer.TestNancyServer
{
    public class PlainModule : NancyModule
    {
        public PlainModule()
        {
            Get["/plaintext"] = _ => "Hello World!";
        }
    }
}
