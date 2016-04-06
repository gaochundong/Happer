using System;
using System.Diagnostics;
using Happer.Hosting.Self;
using Happer.Rpc;
using Logrila.Logging.NLogIntegration;

namespace Happer.TestHttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            Func<Type, object> container = (t) =>
            {
                return new HelloRpcService();
            };
            var rpcServiceResolver = new RpcServiceResolver(container);
            var rpc = new TestRpcModule(rpcServiceResolver);
            rpc.RegisterRpcService(rpcServiceResolver.GetRpcService<HelloRequest, HelloResponse>());

            var bootstrapper = new Bootstrapper();
            bootstrapper.Modules.Add(new TestModule());
            bootstrapper.Modules.Add(rpc);
            bootstrapper.WebSocketModules.Add(new TestWebSocketModule());

            var engine = bootstrapper.Boot();

            string uri = "http://localhost:3202/";
            var host = new SelfHost(engine, new Uri(uri));
            host.Start();
            Console.WriteLine("Server is listening on [{0}].", uri);

            //AutoNavigateTo(uri);

            Console.ReadKey();
            Console.WriteLine("Stopped. Goodbye!");
        }

        private static void AutoNavigateTo(string uri)
        {
            try
            {
                Process.Start(uri);
            }
            catch (Exception) { }
        }
    }
}
