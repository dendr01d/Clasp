
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeTypes.TypeConstraints
{
    internal sealed record TypeSubOfType(SchemeType SubType, SchemeType SuperType, ISemAstNode Node) : TypeConstraint(Node)
    {
        public override string AsString => $"{SubType} ⊆{SuperType}";
    }
}
