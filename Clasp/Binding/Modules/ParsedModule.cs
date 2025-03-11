using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Binding.Modules
{
    internal abstract class ParsedModule : Module
    {
        public readonly Identifier[] ExportedIds;
        public readonly Scope ExportedScope;

        protected ParsedModule(string name, Identifier[] exportedIds, Scope exportedScope) : base(name)
        {
            ExportedIds = exportedIds;
            ExportedScope = exportedScope;
        }
    }
}
