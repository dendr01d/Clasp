namespace ClaspCompiler.SchemeTypes
{
    internal sealed record ListOfType(SchemeType RepeatingType) : SchemeType
    {
        //public UnionType AsUnion { get; init; } = new(AtomicType.Nil, this);
        public override string AsString => $"{RepeatingType}*";
    }
}
