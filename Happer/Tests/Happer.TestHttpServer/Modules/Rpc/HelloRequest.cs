using Happer.Rpc;

namespace Happer.TestHttpServer
{
    [RpcMethod("hello")]
    public class HelloRequest
    {
        public string Name { get; set; }
    }
}
