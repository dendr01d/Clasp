using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemAnnotated : ISemExp
    {
        public IVisibleTypePredicate? VisiblePredicate { get; }
        public SchemeType Type { get; }
    }
}