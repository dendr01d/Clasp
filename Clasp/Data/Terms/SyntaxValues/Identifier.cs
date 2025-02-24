using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Text;

namespace Clasp.Data.Terms.SyntaxValues
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

        public bool TryRenameAsVariable(int phase, out Identifier bindingId)
        {
            bindingId = new Identifier(new GenSym(Name), LexContext);
            ExpansionVarNameBinding binding = new ExpansionVarNameBinding(bindingId, BindingType.Variable);
            return LexContext.TryBind(phase, Name, binding);
        }

        public bool TryRenameAsMacro(int phase, out Identifier bindingId)
        {
            bindingId = new Identifier(new GenSym(Name), LexContext);
            ExpansionVarNameBinding binding = new ExpansionVarNameBinding(bindingId, BindingType.Transformer);
            return LexContext.TryBind(phase, Name, binding);
        }

        //public bool TryResolveBinding(int phase,
        //    [NotNullWhen(true)] out CompileTimeBinding? binding)
        //{
        //    return TryResolveBinding(phase, out binding, out _);
        //}

        public bool TryResolveBinding(int phase,
            [NotNullWhen(true)] out ExpansionVarNameBinding? binding)
        {
            binding = LexContext.ResolveBindings(phase, Name);
            return binding is not null;

            //binding = candidates.Length == 1
            //    ? candidates[0]
            //    : null;

            //return candidates.Length == 1;
        }

        //public override string ToString() => string.Format("#'{0}", _sym);
        protected override string FormatType() => "StxId";
        internal override string DisplayDebug() => string.Format("{0} ({1}): {2}", nameof(Identifier), nameof(Syntax), _sym.ToString());
    }
}
