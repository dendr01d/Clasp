using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding.Environments;

namespace Clasp.Binding.Modules
{
    internal abstract class Module
    {
        public readonly ModuleEnv ModuleEnvironment;

        protected Module(ModuleEnv env)
        {
            ModuleEnvironment = env;
        }
    }
}
