namespace Happer.Rpc
{
    public interface IRpcServiceResolver
    {
        IRpcService<TRequest, TResponse> GetRpcService<TRequest, TResponse>()
            where TRequest : class, new()
            where TResponse : class, new();
    }
}
