using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Text;

namespace Clasp.Data.Terms.Syntax
{
    internal sealed class Identifier : Syntax
    {
        private Symbol _sym;
        public override Symbol Expose() => _sym;

        public string Name => _sym.Name;

        public Identifier(Symbol sym, LexInfo ctx) : base(ctx) => _sym = sym;
        public Identifier(string name, LexInfo ctx) : base(ctx) => _sym = Symbol.Intern(name);
        public Identifier(Token token) : this(token.Text, new LexInfo(token.Location)) { }
        public Identifier(Symbol sym, Syntax copy) : this(sym, copy.LexContext) { }

        public bool TryResolveBinding(int phase, out CompileTimeBinding? binding)
        {
            return LexContext.TryResolveBinding(phase, Name, out binding);
        }

        protected override string FormatType() => "StxId";
    }
}
