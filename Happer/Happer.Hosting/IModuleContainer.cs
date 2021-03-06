﻿using System;
using System.Collections.Generic;
using Happer.Http;

namespace Happer
{
    public interface IModuleContainer
    {
        IEnumerable<Module> GetAllModules();
        Module GetModule(Type moduleType);
    }
}
