namespace ClaspCompiler.SchemeTypes
{
    internal sealed record IntersectionType : SchemeType
    {
        public readonly SchemeType[] ComponentTypes;

        public IntersectionType(params SchemeType[] types)
        {
            ComponentTypes = types;
        }

        public override string AsString => $"({string.Join(" & ", ComponentTypes.AsEnumerable())})";

        public bool Equals(IntersectionType? other) => other is not null && ComponentTypes.SequenceEqual(other.ComponentTypes);
        public override int GetHashCode() => RecursiveHashCode(ComponentTypes);

        private static int RecursiveHashCode(SchemeType[] types)
        {
            if (types.Length == 0)
            {
                return 0;
            }
            else
            {
                return HashCode.Combine(types[0], RecursiveHashCode(types[1..]));
            }
        }
    }
}
