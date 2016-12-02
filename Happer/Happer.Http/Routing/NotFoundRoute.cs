using System.Threading.Tasks;
using Happer.Http.Responses;

namespace Happer.Http.Routing
{
    /// <summary>
    /// Route that is returned when the path could not be matched.
    /// </summary>
    public class NotFoundRoute : Route<Response>
    {
        public NotFoundRoute(string method, string path)
            : base(method, path, null, (x, c) => Task.FromResult<Response>(new NotFoundResponse()))
        {
        }
    }
}
