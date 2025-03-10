using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Binding.Modules
{
    internal sealed class VisitedModule : Module
    {
        public readonly CoreForm ParsedForm;
        public readonly Identifier[] ExportedIds;
        public readonly Scope ExportedScope;

        public override bool Visited => true;
        public override bool Instantiated => false;

        public VisitedModule(string name, CoreForm cf, Identifier[] ids, Scope scp) : base(name)
        {
            ParsedForm = cf;
            ExportedIds = ids;
            ExportedScope = scp;
        }
    }
}
