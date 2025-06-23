namespace ClaspCompiler.SchemeTypes
{
    /// <summary>
    /// The type of a variadic argument, which is a sequence of zero or more objects of type <see cref="Type"/>
    /// </summary>
    internal sealed record VariadType : SchemeType
    {
        public readonly SchemeType Type;
        public VariadType(SchemeType type) => Type = type;
        public override string AsString => $"{Type}*";
    }
}
