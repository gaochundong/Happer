using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Happer.Rpc
{
    public abstract class RpcModule : Happer.Http.Module
    {
        private readonly IRpcServiceResolver _rpcServiceResolver;

        public RpcModule(IRpcServiceResolver rpcServiceResolver)
            : base("/rpc")
        {
            if (rpcServiceResolver == null)
                throw new ArgumentNullException("rpcServiceResolver");

            _rpcServiceResolver = rpcServiceResolver;
        }

        protected void RegisterRpcServices()
        {
            MethodInfo addEndpointMethod =
                typeof(RpcModule)
                .GetMethod("AddEndpoint", BindingFlags.NonPublic | BindingFlags.Instance);

            IEnumerable<Type> services =
                Assembly
                .GetCallingAssembly()
                .GetExportedTypes()
                .Where(t => t.IsSubclassOf(typeof(IRpcService<,>))
                    && !t.IsInterface
                    && !t.IsAbstract);

            foreach (Type service in services)
            {
                var typeArguments =
                    service.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRpcService<,>))
                        .SelectMany(i => i.GetGenericArguments())
                        .ToArray();

                addEndpointMethod
                    .MakeGenericMethod(typeArguments)
                    .Invoke(this, new object[] { _rpcServiceResolver });
            }
        }

        private void AddEndpoint<TRequest, TResponse>(IRpcServiceResolver rpcServiceResolver)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            Post[typeof(TRequest).Name, true] = async (ctx, ct) =>
            {
                var rpcService = rpcServiceResolver.GetRpcService<TRequest, TResponse>();
                var request = Bind<TRequest>();
                return await rpcService.Execute(request, ct);
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
