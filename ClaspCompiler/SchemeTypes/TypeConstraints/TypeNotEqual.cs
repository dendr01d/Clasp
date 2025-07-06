using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeTypes.TypeConstraints
{
    internal sealed record TypeNotEqual(ISemAstNode Node, SchemeType TypeA, SchemeType TypeB) : TypeConstraint(Node)
    {
        public override string AsString => $"{TypeA} ≠: {TypeB}";
    }
}
