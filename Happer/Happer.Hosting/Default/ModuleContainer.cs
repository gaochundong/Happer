using System;
using System.Collections.Generic;
using Happer.Http;

namespace Happer
{
    public class ModuleContainer : IModuleContainer
    {
        private Dictionary<string, Module> _modules = new Dictionary<string, Module>();

        public ModuleContainer()
        {
        }

        public void AddModule(Module module)
        {
            _modules.Add(module.GetType().FullName, module);
        }

        public IEnumerable<Module> GetAllModules()
        {
            return _modules.Values;
        }

        public Module GetModule(Type moduleType)
        {
            return _modules[moduleType.FullName];
        }
    }
}
