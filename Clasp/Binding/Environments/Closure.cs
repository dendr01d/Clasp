using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Exceptions;

namespace Clasp.Binding.Environments
{
    internal class Closure : MutableEnv
    {
        public override RootEnv Root { get; }

        public Closure(MutableEnv pred) : base(pred)
        {
            Root = pred.Root;
        }
    }
}
