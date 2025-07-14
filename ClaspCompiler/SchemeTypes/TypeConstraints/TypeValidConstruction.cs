using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeTypes.TypeConstraints
{
    internal sealed record TypeValidConstruction(SchemeType Type, IEnumerable<SchemeType> Delta, ISemAstNode Node) : TypeConstraint(Node)
    {
        public override string AsString => $"{Type} ∊ [{string.Join(", ", Delta)}]";
    }
}
