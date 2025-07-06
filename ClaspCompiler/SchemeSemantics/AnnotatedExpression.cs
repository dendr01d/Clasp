using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record AnnotatedExpression(ISemExp Expression, SchemeType Type)
        : AnnotatedBase<ISemExp>(Expression, Type)
    { }
}
