
namespace ClaspCompiler.SchemeTypes.TypeConstraints
{
    internal sealed record SubType(uint SourceAstId, SchemeType TypeA, SchemeType TypeB) : TypeConstraint(SourceAstId)
    {
        public override string AsString => TypeB is VarType
            ? $"{TypeB} <: {TypeA}"
            : $"{TypeA} <: {TypeB}";
    }
}
