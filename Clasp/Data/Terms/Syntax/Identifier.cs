using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Metadata;

namespace Clasp.Data.Terms.Syntax
{
    internal sealed class Identifier : Syntax
    {
        private Symbol _sym;
        public override Symbol Expose() => _sym;

        public string SymbolicName => _sym.Name;

        public Identifier(Symbol sym, SourceLocation loc, Syntax? copy = null)
            : base(loc, copy)
        {
            _sym = sym;
        }

        public Identifier(string symbolicName, SourceLocation loc, Syntax? copy = null) 
            : base(loc, copy)
        {
            _sym = Symbol.Intern(symbolicName);
        }

        protected override Identifier DeepCopy() => new Identifier(_sym, Location, this);

        public override bool TryExposeIdentifier(
            [NotNullWhen(true)] out Symbol? sym,
            [NotNullWhen(true)] out string? name)
        {
            sym = _sym;
            name = SymbolicName;
            return true;
        }

        public override string ToString() => string.Format("#'{0}", _sym);
        protected override string FormatType() => "StxId";
    }
}
