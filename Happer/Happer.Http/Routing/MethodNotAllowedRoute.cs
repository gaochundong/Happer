using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Happer.Http.Routing
{
    /// <summary>
    /// Route that is returned when the path could be matched but it was for the wrong request method.
    /// </summary>
    public class MethodNotAllowedRoute : Route<Response>
    {
        public MethodNotAllowedRoute(string path, string method, IEnumerable<string> allowedMethods)
            : base(method, path, null, (x, c) => CreateMethodNotAllowedResponse(allowedMethods))
        {
        }

        private static Task<Response> CreateMethodNotAllowedResponse(IEnumerable<string> allowedMethods)
        {
            var response = new Response();
            response.Headers["Allow"] = string.Join(", ", allowedMethods);
            response.StatusCode = HttpStatusCode.MethodNotAllowed;

            return Task.FromResult(response);
        }
    }
}
