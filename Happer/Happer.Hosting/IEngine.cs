using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Happer.Http;

namespace Happer
{
    public interface IEngine
    {
        Task<Context> HandleHttp(HttpListenerContext httpContext, Uri baseUri, CancellationToken cancellationToken);
    }
}
