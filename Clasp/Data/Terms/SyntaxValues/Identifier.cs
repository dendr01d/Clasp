using System.Diagnostics.CodeAnalysis;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Text;

namespace Clasp.Data.Terms.SyntaxValues
{
    /// <summary>
    /// A syntactic Symbol within the program representing some kind of variable.
    /// </summary>
    internal sealed class Identifier : Syntax
    {
        private readonly ScopeSet _scopes;
        private Symbol _sym;
        public override Symbol Expose() => _sym;

        public string Name => _sym.Name;

        private Identifier(Symbol sym, SourceCode loc, ScopeSet scopes) : base(loc)
        {
            _scopes = scopes;
            _sym = sym;
        }
        public Identifier(Symbol sym, SourceCode loc) : this(sym, loc, new ScopeSet()) { }
        public Identifier(string name, SourceCode loc) : this(Symbol.Intern(name), loc) { }
        public Identifier(Token token) : this(token.Text, token.Location) { }

        public bool SameScopes(Identifier other) => _scopes.SameScopes(other._scopes);

        #region Scope-Adjustment

        public override void AddScope(int phase, params Scope[] scopes) => _scopes.AddScope(phase, scopes);
        public override void FlipScope(int phase, params Scope[] scopes) => _scopes.FlipScope(phase, scopes);
        public override void RemoveScope(int phase, params Scope[] scopes) => _scopes.RemoveScope(phase, scopes);
        public override Syntax StripScopes(int inclusivePhaseThreshold)
        {
            Identifier strippedCopy = new Identifier(_sym, Location);
            strippedCopy._scopes.RestrictPhaseUpTo(inclusivePhaseThreshold);
            return strippedCopy;
        }
        public override ScopeSet GetScopeSet() => new ScopeSet(_scopes);

        #endregion

        #region Rename-Binding

        private bool TryRenameAsType(int phase, BindingType type, out Identifier bindingId)
        {
            bindingId = new Identifier(new GenSym(Name), Location, _scopes);
            RenameBinding binding = new RenameBinding(bindingId, BindingType.Variable);
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

        public bool TryResolveBinding(int phase, [NotNullWhen(true)] out RenameBinding? binding)
        {
            binding = _scopes.ResolveBindings(phase, Name);
            return binding is not null;
        }

        public bool TryResolveBinding(int phase, BindingType expectedType,
            [NotNullWhen(true)] out RenameBinding? binding)
        {
            binding = _scopes.ResolveBindings(phase, Name);
            return binding is not null && binding.BoundType == expectedType;
        }

        protected override string FormatType() => "StxIdentifier";
    }
}
