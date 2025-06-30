using System.Collections.Immutable;

using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed record StxDatum(ISchemeExp Datum, ImmutableHashSet<uint> SurroundingScope, SourceRef Source)
        : SyntaxBase(SurroundingScope, Source)
    {
        public override bool IsAtom => Datum.IsAtom;
        public override bool IsNil => Datum.IsNil;
        public override SchemeType Type => AtomicType.SyntaxData;

        public StxDatum(ISchemeExp datum, SourceRef src) : this(datum, [], src) { }

        public override ISchemeExp Expose() => Datum;
        public override StxDatum AddScopes(IEnumerable<uint> scopeTokens)
            => this with { SurroundingScope = SurroundingScope.Union(scopeTokens) };
        public override StxDatum RemoveScopes(IEnumerable<uint> scopeTokens)
            => this with { SurroundingScope = SurroundingScope.Except(scopeTokens) };
        public override StxDatum FlipScopes(IEnumerable<uint> scopeTokens)
            => this with { SurroundingScope = SurroundingScope.SymmetricExcept(scopeTokens) };
        public override StxDatum ClearScopes() => this with { SurroundingScope = [] };

        public override bool BreaksLine => Datum.BreaksLine;
        public override string AsString => Datum.ToString();
        public override void Print(TextWriter writer, int indent) => writer.Write(Datum, indent);
    }
}
