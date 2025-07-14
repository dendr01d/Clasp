using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeTypes.TypeConstraints
{
    internal sealed record TypeNotVoid(SchemeType Type, ISemAstNode Node) : TypeConstraint(Node)
    {
        public override string AsString => $"{Type} ≠ {AtomicType.Void}";
    }
}
