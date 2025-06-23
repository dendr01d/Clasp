using System.Diagnostics.CodeAnalysis;

using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.Textual;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed class StxDatum : SyntaxBase
    {
        public required ISchemeExp Value { get; init; }

        public override bool IsAtom => Value.IsAtom;
        public override bool IsNil => Value.IsNil;

        public StxDatum(SourceRef src, IEnumerable<uint>? scopeSet = null)
            : base(src, scopeSet ?? [])
        { }

        [SetsRequiredMembers]
        public StxDatum(ISchemeExp datum, SourceRef src, IEnumerable<uint>? scopeSet = null)
            : this(src, scopeSet)
        {
            this.Value = datum;
        }

        public override StxDatum AddScopes(params uint[] ids) => new(Value, Source, ScopeSet.Union(ids));
        public override StxDatum RemoveScopes(params uint[] ids) => new(Value, Source, ScopeSet.Except(ids));
        public override StxDatum FlipScopes(params uint[] ids) => new(Value, Source, ScopeSet.SymmetricExcept(ids));
        public override StxDatum ClearScopes() => new(Value, Source);

        public override bool BreaksLine => Value.BreaksLine;
        public override string AsString => Value.ToString();
        public override void Print(TextWriter writer, int indent) => writer.Write(Value, indent);
    }
}
