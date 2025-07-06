namespace ClaspCompiler.SchemeTypes
{
    internal sealed record FunctionType(SchemeType OutputType, SchemeType InputType) : SchemeType
    {
        public FunctionType(SchemeType outputType, IEnumerable<SchemeType> inputTypes)
            : this(outputType, TypeList.List([.. inputTypes]))
        { }

        public override string AsString => $"({InputType} → {OutputType})";
    }
}