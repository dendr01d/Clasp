using System.Diagnostics.CodeAnalysis;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Text;

namespace Clasp.Data.Terms.SyntaxValues
{
    internal sealed class Identifier : Syntax
    {
        private readonly ScopeSet _scopes;
        private Symbol _sym;
        public override Symbol Expose() => _sym;

        public string Name => _sym.Name;

        public Identifier(Symbol sym, SourceCode loc, ScopeSet scopes) : base(loc)
        {
            _sym = sym;
            _scopes = scopes;
        }
        public Identifier(string name, SourceCode loc, ScopeSet scopes) : this(Symbol.Intern(name), loc, scopes) { }
        public Identifier(Token token) : this(token.Text, token.Location, new ScopeSet()) { }

        #region Scope-Adjustment

        public override void AddScope(int phase, params Scope[] scopes) => _scopes.AddScope(phase, scopes);
        public override void FlipScope(int phase, params Scope[] scopes) => _scopes.FlipScope(phase, scopes);
        public override void RemoveScope(int phase, params Scope[] scopes) => _scopes.RemoveScope(phase, scopes);
        public override Syntax StripScopes(int inclusivePhaseThreshold)
            => new Identifier(_sym, Location, _scopes.RestrictPhaseUpTo(inclusivePhaseThreshold));
        public override ScopeSet GetScopes() => new ScopeSet(_scopes);

        #endregion

        public override SyntaxPair ListPrepend(Syntax stx) => new SyntaxPair(stx, this, Location, _scopes);

        #region Rename-Binding

        private bool TryRenameAsType(int phase, BindingType type, out Identifier bindingId)
        {
            bindingId = new Identifier(new GenSym(Name), Location, );
            ExpansionVarNameBinding binding = new ExpansionVarNameBinding(bindingId, BindingType.Variable);
            return _scopes.TryBind(phase, Name, binding);
        }

        public bool TryRenameAsVariable(int phase, out Identifier bindingId)
            => TryRenameAsType(phase, BindingType.Variable, out bindingId);

        public bool TryRenameAsMacro(int phase, out Identifier bindingId)
            => TryRenameAsType(phase, BindingType.Transformer, out bindingId);

        public bool TryRenameAsModule(int phase, out Identifier bindingId)
            => TryRenameAsType(phase, BindingType.Module, out bindingId);

        #endregion

        //public bool TryResolveBinding(int phase,
        //    [NotNullWhen(true)] out CompileTimeBinding? binding)
        //{
        //    return TryResolveBinding(phase, out binding, out _);
        //}

        public bool TryResolveBinding(int phase, [NotNullWhen(true)] out ExpansionVarNameBinding? binding)
        {
            binding = _scopes.ResolveBindings(phase, Name);
            return binding is not null;
        }

        protected override string FormatType() => "StxIdentifier";
    }
}
