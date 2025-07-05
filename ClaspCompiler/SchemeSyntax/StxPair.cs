using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed record StxPair(ImmutableHashSet<uint> SurroundingScope, SourceRef Source)
        : SyntaxBase(SurroundingScope, Source), ICons<ISyntax>
    {
        private Lazy<ISyntax> _lazyCar { get; [MemberNotNull(nameof(Car))] set; }
        private Lazy<ISyntax> _lazyCdr { get; [MemberNotNull(nameof(Cdr))] set; }
        private Lazy<ISchemeExp> _lazyCons => new(() => new Cons(_lazyCar.Value, _lazyCdr.Value));

        public required ISyntax Car
        {
            get => _lazyCar.Value;
            [MemberNotNull(nameof(_lazyCar))]
            init => _lazyCar = new(value);
        }
        public required ISyntax Cdr
        {
            get => _lazyCdr.Value;
            [MemberNotNull(nameof(_lazyCdr))]
            init => _lazyCdr = new(value);
        }

        public override bool IsAtom => false;
        public override bool IsNil => false;
        public override SchemeType Type => AtomicType.SyntaxPair;

        public StxPair(ISyntax basis)
            : this(basis.SurroundingScope, basis.Source) { }

        public StxPair(SourceRef src)
            : this([], src)
        { }

        public override ISchemeExp Expose() => _lazyCons.Value;
        public override StxPair AddScopes(IEnumerable<uint> scopeTokens) => this with
        {
            SurroundingScope = SurroundingScope.Union(scopeTokens),
            _lazyCar = new(() => Car.AddScopes(scopeTokens)),
            _lazyCdr = new(() => Cdr.AddScopes(scopeTokens))
        };
        public override StxPair RemoveScopes(IEnumerable<uint> scopeTokens) => this with
        {
            SurroundingScope = SurroundingScope.Except(scopeTokens),
            _lazyCar = new(() => Car.RemoveScopes(scopeTokens)),
            _lazyCdr = new(() => Cdr.RemoveScopes(scopeTokens))
        };
        public override StxPair FlipScopes(IEnumerable<uint> scopeTokens) => this with
        {
            SurroundingScope = SurroundingScope.Except(scopeTokens),
            _lazyCar = new(() => Car.RemoveScopes(scopeTokens)),
            _lazyCdr = new(() => Cdr.RemoveScopes(scopeTokens))
        };
        public override StxPair ClearScopes() => this with
        {
            SurroundingScope = [],
            _lazyCar = new(Car.ClearScopes),
            _lazyCdr = new(Cdr.ClearScopes)
        };

        public override bool BreaksLine => Car.BreaksLine || Cdr is ICons<ISyntax>;
        public override string AsString => this.Stringify();
        public override void Print(TextWriter writer, int indent) => writer.WriteCons(this, indent);

        public IEnumerator<ISyntax> GetEnumerator() => this.Enumerate();
        IEnumerator IEnumerable.GetEnumerator() => this.Enumerate();
    }
}
