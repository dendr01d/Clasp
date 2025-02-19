using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;

namespace Clasp.Binding.Environments
{
    internal class Closure : MutableEnv
    {
        public override ClaspEnvironment Predecessor { get; }
        public override RuntimeEnv Runtime { get; }

        public Closure(MutableEnv predecessor) : base()
        {
            Predecessor = predecessor;
            Runtime = predecessor.Runtime;
        }
    }
}
