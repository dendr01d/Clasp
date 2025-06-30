using System.Collections.Immutable;

using ClaspCompiler.LexicalScope;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed record Identifier(Symbol Symbol, ImmutableHashSet<uint> SurroundingScope, SourceRef Source) : SyntaxBase(SurroundingScope, Source)
    {
        public Binding? BindingInfo { get; init; } = null;
        public Symbol ExpandedSymbol => BindingInfo?.UniqueName ?? Symbol;

        public override bool IsAtom => true;
        public override bool IsNil => false;
        public override SchemeType Type => AtomicType.Identifier;

        public Identifier(Symbol sym, SourceRef src) : this(sym, [], src) { }

        public override Symbol Expose() => Symbol;
        public override Identifier AddScopes(IEnumerable<uint> scopeTokens)
            => this with { SurroundingScope = SurroundingScope.Union(scopeTokens) };
        public override Identifier RemoveScopes(IEnumerable<uint> scopeTokens)
            => this with { SurroundingScope = SurroundingScope.Except(scopeTokens) };
        public override Identifier FlipScopes(IEnumerable<uint> scopeTokens)
            => this with { SurroundingScope = SurroundingScope.SymmetricExcept(scopeTokens) };
        public override Identifier ClearScopes() => this with { SurroundingScope = [] };

        public override bool BreaksLine => false;
        public override string AsString => Symbol.AsString;
        public override void Print(TextWriter writer, int indent) => writer.Write(AsString);
    }
}
