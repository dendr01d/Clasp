using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemLiteral : ISemExp
    {
        public SchemeType Type { get; }
    }
}
