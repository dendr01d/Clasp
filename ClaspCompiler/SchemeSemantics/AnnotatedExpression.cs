using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record AnnotatedExpression(ISemExp Expression, SchemeType Type, IVisibleTypePredicate? pred = null)
        : AnnotatedBase<ISemExp>(Expression, Type)
    {
        public override IVisibleTypePredicate? VisiblePredicate { get; } = pred;
    }
}