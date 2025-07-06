using System.Collections.Immutable;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record UnionType(ImmutableHashSet<SchemeType> Types) : SchemeType
    {
        private readonly Lazy<int> _lazyHash = new(() => CreateVariadicHash(nameof(UnionType), Types));

        public UnionType(params SchemeType[] types) : this(types.ToImmutableHashSet()) { }
        public UnionType(IEnumerable<SchemeType> types) : this(types.ToImmutableHashSet()) { }

        public override string AsString => $"(U{string.Concat(Types.Select(x => $" {x}"))})";

        public bool Equals(UnionType? other) => other is not null && Types.SetEquals(other.Types);
        public override int GetHashCode() => _lazyHash.Value;

        public static readonly UnionType Number = new(AtomicType.Integer);
        public static readonly UnionType Boole = new(AtomicType.True, AtomicType.False);
        public static readonly UnionType Syntax = new(AtomicType.Identifier, AtomicType.SyntaxPair, AtomicType.SyntaxData);
    }
}
