using System;
using System.Collections.Generic;

namespace Happer.Http
{
    public class ModuleCatalog
    {
        private Func<IEnumerable<Module>> _getAllModules;
        private Func<Type, Module> _getModule;

        public ModuleCatalog(Func<IEnumerable<Module>> getAllModules, Func<Type, Module> getModule)
        {
            _getAllModules = getAllModules;
            _getModule = getModule;
        }

        public IEnumerable<Module> GetAllModules()
        {
            return _getAllModules();
        }

        public Module GetModule(Type moduleType)
        {
            return _getModule(moduleType);
        }
    }
}
