using System;
using System.Collections.Generic;
using Happer.Http;
using Happer.Http.WebSockets;

namespace Happer.TestHttpServer
{
    public class TestContainer : IModuleContainer
    {
        private Dictionary<string, Module> _modules = new Dictionary<string, Module>();
        private Dictionary<string, WebSocketModule> _webSocketModules = new Dictionary<string, WebSocketModule>();

        public TestContainer()
        {
        }

        public void AddModule(Module module)
        {
            _modules.Add(module.GetType().FullName, module);
        }

        public void AddWebSocketModule(WebSocketModule module)
        {
            _webSocketModules.Add(module.GetType().FullName, module);
        }

        public IEnumerable<Module> GetAllModules()
        {
            return _modules.Values;
        }

        public Module GetModule(Type moduleType)
        {
            return _modules[moduleType.FullName];
        }

        public IEnumerable<WebSocketModule> GetAllWebSocketModules()
        {
            return _webSocketModules.Values;
        }

        public WebSocketModule GetWebSocketModule(Type moduleType)
        {
            return _webSocketModules[moduleType.FullName];
        }
    }
}
