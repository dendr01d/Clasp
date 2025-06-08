using System.Collections.Immutable;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record FunctionType : SchemeType
    {
        public ImmutableArray<SchemeType> ArgumentTypes { get; init; }
        public SchemeType ResultType { get; init; }

        public FunctionType(SchemeType outType, params SchemeType[] inTypes)
        {
            ArgumentTypes = inTypes.ToImmutableArray();
            ResultType = outType;
        }

        public bool Equals(FunctionType? other)
        {
            return other is not null
                && other.ResultType == ResultType
                && other.ArgumentTypes.SequenceEqual(ArgumentTypes);
        }
        public override int GetHashCode() => HashCode.Combine(ArgumentTypes, ResultType);

        public override string AsString => $"(({string.Join(' ', ArgumentTypes)}) . {ResultType})";
    }
}
