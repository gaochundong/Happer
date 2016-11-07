using System;
using System.Collections.Generic;
using Happer.Http;

namespace Happer.TestHttpServer
{
    public interface IModuleContainer
    {
        IEnumerable<Module> GetAllModules();
        Module GetModule(Type moduleType);
    }
}
