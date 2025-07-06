using System.Collections.Immutable;

namespace ClaspCompiler.SchemeTypes
{
    //internal sealed record ProductType(ImmutableArray<SchemeType> Types) : SchemeType
    //{
    //    private readonly Lazy<int> _lazyHash = new(() => CreateVariadicHash(nameof(ProductType), Types));

    //    public ProductType(params SchemeType[] types) : this(types.ToImmutableArray()) { }
    //    public ProductType(IEnumerable<SchemeType> types) : this(types.ToImmutableArray()) { }

    //    public override string AsString => $"(π{string.Concat(Types.Select(x => $" {x}"))})";

    //    public bool Equals(UnionType? other) => other is not null && Types.SequenceEqual(other.Types);
    //    public override int GetHashCode() => _lazyHash.Value;
    //}
}
