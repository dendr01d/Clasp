using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeTypes.TypeConstraints
{
    internal sealed record TypeEqualsType(SchemeType TypeA, SchemeType TypeB, ISemAstNode Node) : TypeConstraint(Node)
    {
        public override string AsString => $"{TypeA} = {TypeB}";
    }
}
