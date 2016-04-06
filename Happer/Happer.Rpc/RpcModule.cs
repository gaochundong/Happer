using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Happer.Rpc
{
    public abstract class RpcModule : Happer.Http.Module
    {
        private readonly IRpcServiceResolver _rpcServiceResolver;

        public RpcModule(IRpcServiceResolver rpcServiceResolver)
        {
            if (rpcServiceResolver == null)
                throw new ArgumentNullException("rpcServiceResolver");

            _rpcServiceResolver = rpcServiceResolver;
        }

        public RpcModule(string modulePath, IRpcServiceResolver rpcServiceResolver)
            : base(modulePath)
        {
            if (rpcServiceResolver == null)
                throw new ArgumentNullException("rpcServiceResolver");

            _rpcServiceResolver = rpcServiceResolver;
        }

        public void RegisterRpcService<TRequest, TResponse>(IRpcService<TRequest, TResponse> service)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            MethodInfo addEndpointMethod =
                typeof(RpcModule)
                .GetMethod("AddEndpoint", BindingFlags.NonPublic | BindingFlags.Instance);

            var typeArguments =
                service
                .GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRpcService<,>))
                .SelectMany(i => i.GetGenericArguments())
                .ToArray();

            addEndpointMethod
                .MakeGenericMethod(typeArguments)
                .Invoke(this, new object[] { _rpcServiceResolver });
        }

        private void AddEndpoint<TRequest, TResponse>(IRpcServiceResolver rpcServiceResolver)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            Post["/" + typeof(TRequest).Name, true] = async (ctx, ct) =>
            {
                var rpcService = rpcServiceResolver.GetRpcService<TRequest, TResponse>();
                var request = Bind<TRequest>();
                var response = await rpcService.Execute(request, ct);
                return JsonConvert.SerializeObject(response);
            };
        }

        private TRequest Bind<TRequest>()
            where TRequest : class, new()
        {
            this.Request.Body.Position = 0;

            string bodyText;
            using (var bodyReader = new StreamReader(this.Request.Body))
            {
                bodyText = bodyReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<TRequest>(bodyText);
        }
    }
}
