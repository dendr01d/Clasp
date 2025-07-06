using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemAnnotated : ISemExp
    {
        public SchemeType Type { get; }
    }
}
