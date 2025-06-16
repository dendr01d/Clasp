using System.Collections.Immutable;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record ProductType : SchemeType
    {
        public readonly ImmutableArray<SchemeType> Types;

        public ProductType(IEnumerable<SchemeType> types) => Types = types.ToImmutableArray();
        public ProductType(params SchemeType[] types) => Types = types.ToImmutableArray();

        public bool Equals(ProductType? other) => Types.SequenceEqual(other?.Types ?? []);
        public override int GetHashCode() => Types.GetHashCode();

        public override string AsString => $"({string.Join(" * ", Types)})";
    }
}
