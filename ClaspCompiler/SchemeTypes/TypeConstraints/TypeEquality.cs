using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeTypes.TypeConstraints
{
    internal sealed record TypeEquality(ISemAstNode Node, SchemeType TypeA, SchemeType TypeB) : TypeConstraint(Node)
    {
        public override string AsString => TypeB is VarType
            ? $"{TypeB} =: {TypeA}"
            : $"{TypeA} =: {TypeB}";
    }
}
