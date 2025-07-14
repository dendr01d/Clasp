namespace ClaspCompiler.SchemeTypes
{
    internal sealed record DottedPreType(SchemeType[] Types) : SchemeType
    {
        public override string AsString => $"({string.Join(' ', Types.AsEnumerable())})";
    }
}
