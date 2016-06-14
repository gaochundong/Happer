using Happer.Rpc;

namespace Happer.TestHttpServer
{
    public class TestRpcModule : RpcModule
    {
        public TestRpcModule(IRpcServiceResolver rpcServiceResolver)
            : base("rpc", rpcServiceResolver)
        {
        }
    }
}
