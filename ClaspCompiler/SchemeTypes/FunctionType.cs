using System.Collections.Immutable;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record FunctionType(SchemeType OutputType, ImmutableArray<SchemeType> InputTypes) : SchemeType
    {
        public FunctionType(SchemeType outputType, params SchemeType[] inputTypes)
            : this(outputType, inputTypes.ToImmutableArray())
        { }

        public bool Equals(FunctionType? other) => OutputType == other?.OutputType && InputTypes.SequenceEqual(other.InputTypes);
        public override int GetHashCode() => HashCode.Combine(OutputType, InputTypes);

        public override string AsString => $"Fun<{OutputType}; {string.Join(", ", InputTypes)}>";
    }
}