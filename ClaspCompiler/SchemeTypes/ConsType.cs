namespace ClaspCompiler.SchemeTypes
{
    internal sealed record ConsType(SchemeType CarType, SchemeType CdrType) : SchemeType
    {
        public override string AsString => $"Cons<{CarType}, {CdrType}>";
    }
}
