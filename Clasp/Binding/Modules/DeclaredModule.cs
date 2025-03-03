using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Binding.Modules
{
    internal sealed class DeclaredModule : Module
    {
        public readonly Syntax FreshSyntax;
        public override bool Visited => false;
        public override bool Instantiated => false;

        public DeclaredModule(string name, Syntax stx) : base(name)
        {
            FreshSyntax = stx;
        }

    }
}
