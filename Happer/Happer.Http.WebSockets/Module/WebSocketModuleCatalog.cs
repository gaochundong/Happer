using System;
using System.Collections.Generic;

namespace Happer.Http.WebSockets
{
    public class WebSocketModuleCatalog
    {
        private Func<IEnumerable<WebSocketModule>> _getAllModules;
        private Func<Type, WebSocketModule> _getModule;

        public WebSocketModuleCatalog(Func<IEnumerable<WebSocketModule>> getAllModules, Func<Type, WebSocketModule> getModule)
        {
            _getAllModules = getAllModules;
            _getModule = getModule;
        }

        public IEnumerable<WebSocketModule> GetAllModules()
        {
            return _getAllModules();
        }

        public WebSocketModule GetModule(Type moduleType)
        {
            return _getModule(moduleType);
        }
    }
}
