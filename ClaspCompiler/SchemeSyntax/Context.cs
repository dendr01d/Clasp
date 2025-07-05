using System.Collections.Immutable;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed record Context
    {
        public SourceRef Source { get; init; }
        public ImmutableHashSet<uint> Scope { get; private set; }

        public Context(SourceRef src, IEnumerable<uint> scope)
        {
            Source = src;
            Scope = scope.ToImmutableHashSet();
        }

        public Context(SourceRef src) : this(src, []) { }

        public void AddScopes(params uint[] scopes) => Scope = Scope.Union(scopes);
        public void RemoveScopes(params uint[] scopes) => Scope = Scope.Except(scopes);
        public void FlipScopes(params uint[] scopes) => Scope = Scope.SymmetricExcept(scopes);
        public void ClearScopes() => Scope = [];
    }
}
