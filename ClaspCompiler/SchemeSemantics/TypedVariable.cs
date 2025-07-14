using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record TypedVariable(Variable Variable, SchemeType Type)
        : AnnotatedBase<Variable>(Variable, Type), ISemVar, IVisibleTypePredicate
    {
        public override IVisibleTypePredicate? VisiblePredicate => this;

        public string Name => Variable.Name;
    }
}