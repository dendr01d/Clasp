using System.Collections.Immutable;

using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSyntax.Abstract
{
    internal abstract record SyntaxBase(ImmutableHashSet<uint> SurroundingScope, SourceRef Source) : ISyntax
    {
        public abstract bool IsAtom { get; }
        public abstract bool IsNil { get; }
        public bool IsFalse => false;
        public abstract SchemeType Type { get; }

        public abstract ISchemeExp Expose();
        public abstract ISyntax AddScopes(IEnumerable<uint> scopeTokens);
        public abstract ISyntax RemoveScopes(IEnumerable<uint> scopeTokens);
        public abstract ISyntax FlipScopes(IEnumerable<uint> scopeTokens);
        public abstract ISyntax ClearScopes();

        public abstract bool BreaksLine { get; }
        public abstract string AsString { get; }
        public abstract void Print(TextWriter writer, int indent);
        public sealed override string ToString() => AsString;
    }
}
