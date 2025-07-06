using System.Collections.Immutable;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record UnionType(ImmutableHashSet<SchemeType> Types) : SchemeType
    {
        private readonly Lazy<int> _lazyHash = new(() => CreateVariadicHash(nameof(UnionType), Types));

        public string? NameOverride { private get; init; } = null;

        public UnionType(params SchemeType[] types) : this(types.ToImmutableHashSet()) { }
        public UnionType(IEnumerable<SchemeType> types, string? nameOverride = null) : this(types.ToImmutableHashSet())
        {
            NameOverride = nameOverride;
        }

        public override string AsString => NameOverride ?? $"(U{string.Concat(Types.Select(x => $" {x}"))})";

        public bool Equals(UnionType? other) => other is not null && Types.SetEquals(other.Types);
        public override int GetHashCode() => _lazyHash.Value;

    }
}
