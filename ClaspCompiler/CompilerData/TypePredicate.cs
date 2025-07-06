using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.CompilerData
{
    internal sealed class TypePredicate : PrimitiveOperator
    {
        public SchemeType Permittee { get; init; }

        public TypePredicate(AtomicType permittee, string? name = null)
            : base(name ?? $"{permittee.ToString().ToLower()}?", SchemeType.PredicateFunction, false)
        {
            Permittee = permittee;
        }

        public TypePredicate(SchemeType permittee, string name)
            : base(name, SchemeType.PredicateFunction, false)
        {
            Permittee = permittee;
        }
    }
}
