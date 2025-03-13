using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Binding.Modules
{
    internal sealed class VisitedModule : ParsedModule
    {
        public readonly CoreForm ParsedForm;

        public override bool Visited => true;
        public override bool Instantiated => false;

        public VisitedModule(string name, CoreForm cf, Identifier[] ids, Scope scp)
            : base(name, ids, scp)
        {
            ParsedForm = cf;
        }
    }
}
