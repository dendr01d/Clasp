namespace ClaspCompiler.SchemeTypes
{
    internal sealed record FunctionType : SchemeType
    {
        public SchemeType ArgumentType { get; init; }
        public SchemeType OutputType { get; init; }

        public FunctionType(SchemeType argType, SchemeType outType)
        {
            ArgumentType = argType;
            OutputType = outType;
        }

        public override string AsString => $"({ArgumentType} → {OutputType})";
    }
}
