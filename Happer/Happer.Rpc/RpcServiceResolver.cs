using System;

namespace Happer.Rpc
{
    public class RpcServiceResolver : IRpcServiceResolver
    {
        private readonly Func<Type, object> _getService;

        public RpcServiceResolver(Func<Type, object> container)
        {
            _getService = container;
        }

        public IRpcService<TRequest, TResponse> GetRpcService<TRequest, TResponse>()
            where TRequest : class, new()
            where TResponse : class, new()
        {
            return (IRpcService<TRequest, TResponse>)_getService(typeof(IRpcService<TRequest, TResponse>));
        }
    }
}
