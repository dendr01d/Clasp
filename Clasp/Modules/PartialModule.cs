using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Modules
{
    /// <summary>
    /// The partially-expanded syntax of an imported module form.
    /// </summary>
    internal sealed class PartialModule : InstantiatedModule
    {
        public readonly Syntax PartiallyExpandedSyntax;
        private readonly Scope _insideEdge;

        private readonly ExpansionContext _context;

        private PartialModule(string name, Syntax stx, Identifier[] exports,
            Scope outerEdge, Scope innerEdge, ExpansionContext context)
            : base(name, outerEdge, exports)
        {
            PartiallyExpandedSyntax = stx;
            _insideEdge = innerEdge;
            _context = context;
        }

        public static PartialModule VisitFresh(FreshModule fresh)
        {

            //???
            return new PartialModule();
        }
    }
}
