using System;
using System.Collections.Generic;
using Happer.Http;
using Happer.Http.WebSockets;

namespace Happer.TestHttpServer
{
    public interface IModuleContainer
    {
        IEnumerable<Module> GetAllModules();
        Module GetModule(Type moduleType);

        IEnumerable<WebSocketModule> GetAllWebSocketModules();
        WebSocketModule GetWebSocketModule(Type moduleType);
    }
}
