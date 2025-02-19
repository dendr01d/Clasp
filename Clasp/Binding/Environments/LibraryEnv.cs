using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;

namespace Clasp.Binding.Environments
{
    internal abstract class LibraryEnv : ClaspEnvironment
    {
        public override RuntimeEnv Runtime { get; }
        public abstract Scope ImplicitScope { get; }

        protected LibraryEnv(RuntimeEnv runtime)
        {
            Runtime = runtime;
        }
    }
}
