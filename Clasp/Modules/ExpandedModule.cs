using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding.Environments;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Process;

namespace Clasp.Modules
{
    internal sealed class ExpandedModule : ProcessedModule
    {
        public readonly Syntax ExpandedBody;

        private ExpandedModule(string name, Identifier[] ids, Syntax stx) : base(name, ids)
        {
            ExpandedBody = stx;
        }

        public static ExpandedModule Expand(FreshModule fm)
        {
            Syntax expanded = Expander.ExpandModuleSyntax(fm.FreshModuleForm, out Identifier[] ids);
            return new ExpandedModule(fm.Name, ids, expanded);
        }
    }
}
