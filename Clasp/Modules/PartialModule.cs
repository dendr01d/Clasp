using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding;
using Clasp.Binding.Environments;
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

        private readonly Dictionary<int, RootEnv> _compileTimeEnvironments;

        private PartialModule(string name, Syntax stx, Identifier[] exports,
            Scope outerEdge, Scope innerEdge, IEnumerable<KeyValuePair<int, RootEnv>> ctes)
            : base(name, outerEdge, exports)
        {
            PartiallyExpandedSyntax = stx;
            _insideEdge = innerEdge;

            _compileTimeEnvironments = new Dictionary<int, RootEnv>(ctes);
        }

        public static PartialModule VisitFresh(FreshModule fresh)
        {

            //???
            return new PartialModule();
        }
    }
}
