using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;

namespace Clasp.Binding.Modules
{
    internal sealed class InstantiatedModule : Module
    {
        public readonly RootEnv EnrichedEnvironment;
        public override bool Visited => true;
        public override bool Instantiated => true;

        public InstantiatedModule(string name, RootEnv env) : base(name)
        {
            EnrichedEnvironment = env;
        }
    }
}
