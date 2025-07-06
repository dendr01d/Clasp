namespace ClaspCompiler.SchemeTypes.TypeConstraints
{
    internal sealed record TypeNotEqual(uint SourceAstId, SchemeType TypeA, SchemeType TypeB) : TypeConstraint(SourceAstId)
    {
        public override string AsString => $"{TypeA} ╪ {TypeB}";
    }
}
