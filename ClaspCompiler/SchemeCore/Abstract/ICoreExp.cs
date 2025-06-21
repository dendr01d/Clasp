using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeCore.Abstract
{
    internal interface ICoreExp : IPrintable
    {
        public SchemeType Type { get; }
    }
}
