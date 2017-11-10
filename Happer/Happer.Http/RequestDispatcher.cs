using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Happer.Http.Routing;

namespace Happer.Http
{
    public class RequestDispatcher
    {
        private readonly RouteResolver _routeResolver;
        private readonly RouteInvoker _routeInvoker;

        public RequestDispatcher(RouteResolver routeResolver, RouteInvoker routeInvoker)
        {
            _routeResolver = routeResolver;
            _routeInvoker = routeInvoker;
        }

        public async Task<Response> Dispatch(Context context, CancellationToken cancellationToken)
        {
            var resolveResult = Resolve(context);

            context.Parameters = resolveResult.Parameters;
            context.ResolvedRoute = resolveResult.Route;

            return await _routeInvoker.Invoke(resolveResult.Route, cancellationToken, resolveResult.Parameters, context).ConfigureAwait(false);
        }

        private ResolveResult Resolve(Context context)
        {
            var originalAcceptHeaders = context.Request.Headers.Accept;
            var originalRequestPath = context.Request.Path;

            return InvokeRouteResolver(context, originalRequestPath, originalAcceptHeaders);
        }

        private ResolveResult InvokeRouteResolver(Context context, string path, IEnumerable<Tuple<string, decimal>> acceptHeaders)
        {
            context.Request.Headers.Accept = acceptHeaders;
            context.Request.Url.Path = path;

            return _routeResolver.Resolve(context);
        }
    }
}
