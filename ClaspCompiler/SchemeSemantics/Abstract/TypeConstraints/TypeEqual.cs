using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics.Abstract.TypeConstraints
{
    internal sealed class TypeEqual : TypeConstraint
    {
        public SchemeType TypeA { get; init; }
        public SchemeType TypeB { get; init; }

        public TypeEqual(ISemExp src, SchemeType typeA, SchemeType typeB)
            : base(src)
        {
            TypeA = typeA;
            TypeB = typeB;
        }

        public override string AsString => $"(== {TypeA} {TypeB})";
    }
}
