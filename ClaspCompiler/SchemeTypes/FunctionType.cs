namespace ClaspCompiler.SchemeTypes
{
    internal sealed record FunctionType : SchemeType
    {
        public SchemeType ParametersType { get; init; }
        public SchemeType ResultType { get; init; }

        public FunctionType(SchemeType resultType, SchemeType paramsType)
        {
            ParametersType = paramsType;
            ResultType = resultType;
        }

        public FunctionType(SchemeType resultType, params SchemeType[] paramsTypes)
            : this(resultType, new IntersectionType(paramsTypes))
        { }

        public bool Equals(FunctionType? other)
        {
            return other is not null
                && other.ResultType == ResultType
                && other.ParametersType == ParametersType;
        }
        public override int GetHashCode() => HashCode.Combine(ParametersType, ResultType);

        public override string AsString => $"(({string.Join(' ', ParametersType)}) -> {ResultType})";
    }
}