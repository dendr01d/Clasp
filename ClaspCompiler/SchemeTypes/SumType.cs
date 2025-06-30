using System.Collections.Immutable;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record SumType(ImmutableHashSet<SchemeType> Types) : SchemeType
    {
        public bool Equals(SumType? other) => Types.SetEquals(other?.Types ?? []);
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

        public static readonly SumType Numeric = new([AtomicType.Integer]);
        public static readonly SumType Syntax = new([AtomicType.Identifier, AtomicType.SyntaxPair, AtomicType.SyntaxData]);
        public static readonly SumType List = new([AtomicType.Nil, new ConsType(AtomicType.Any, AtomicType.Any)]);

    }
}
