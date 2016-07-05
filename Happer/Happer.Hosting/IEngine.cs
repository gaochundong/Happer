using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Happer
{
    public interface IEngine
    {
        Task HandleHttp(HttpListenerContext httpContext, Uri baseUri, CancellationToken cancellationToken);
    }
}
