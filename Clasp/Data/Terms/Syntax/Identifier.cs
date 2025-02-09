using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Metadata;
using Clasp.Data.Text;

namespace Clasp.Data.Terms.Syntax
{
    internal sealed class Identifier : Syntax
    {
        private Symbol _sym;
        public override Symbol Expose() => _sym;

        public string Name => _sym.Name;

        public Identifier(Symbol sym, LexInfo ctx) : base(ctx)
        {
            _sym = sym;
        }

        public Identifier(string symbolicName, LexInfo ctx) : base(ctx)
        {
            _sym = Symbol.Intern(symbolicName);
        }

        public Identifier(Token token) : this(token.Text, new LexInfo(token.Location)) { }

        public Identifier(Symbol sym, Syntax copy) : this(sym, copy.LexContext) { }

        public static Identifier Implicit(Symbol sym) => new Identifier(sym, LexInfo.Innate);

        protected override Identifier DeepCopy() => new Identifier(_sym, this);

        public override string ToString() => string.Format("#'{0}", _sym);
        protected override string FormatType() => "StxId";
    }
}
