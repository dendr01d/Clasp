using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding.Environments;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Modules
{
    /// <summary>
    /// The syntax of a freshly-imported module form, without any processing work done.
    /// </summary>
    internal sealed class FreshModule : Module
    {
        public readonly Syntax FreshSyntax;

        public FreshModule(string name, Syntax stx) : base(name)
        {
            FreshSyntax = stx;
        }
    }
}
