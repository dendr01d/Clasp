using System.Collections.Immutable;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record SumType : SchemeType
    {
        public readonly ImmutableHashSet<SchemeType> Types;

        public SumType(IEnumerable<SchemeType> types) => Types = types.ToImmutableHashSet();
        public SumType(params SchemeType[] types) => Types = types.ToImmutableHashSet();

        public bool Equals(ProductType? other) => Types.SetEquals(other?.Types ?? []);
        public override int GetHashCode() => Types.GetHashCode();

        public override string AsString => $"({string.Join(" + ", Types)})";

        public static SchemeType Sum(params SchemeType[] types)
        {
            HashSet<SchemeType> uniqueTypes = [.. types];

            if (uniqueTypes.Count == 1)
            {
                return uniqueTypes.First();
            }
            else
            {
                return new SumType([.. uniqueTypes]);
            }
        }
    }
}
