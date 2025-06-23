using System.Collections.Immutable;

using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Textual;

namespace ClaspCompiler.SchemeSyntax.Abstract
{
    internal abstract class SyntaxBase : ISyntax
    {
        public SourceRef Source { get; init; }
        public ImmutableHashSet<uint> ScopeSet { get; init; }

        public SchemeType Type => AtomicType.Syntax;
        public abstract bool IsAtom { get; }
        public abstract bool IsNil { get; }

        protected SyntaxBase(SourceRef src, IEnumerable<uint> scopeSet)
        {
            Source = src;
            ScopeSet = [.. scopeSet];
        }

        public abstract ISyntax AddScopes(params uint[] ids);
        public abstract ISyntax RemoveScopes(params uint[] ids);
        public abstract ISyntax FlipScopes(params uint[] ids);
        public abstract ISyntax ClearScopes();

        public abstract bool BreaksLine { get; }
        public abstract string AsString { get; }
        public abstract void Print(TextWriter writer, int indent);
        public sealed override string ToString() => AsString;
    }
}
