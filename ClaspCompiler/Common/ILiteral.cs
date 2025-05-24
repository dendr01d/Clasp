using ClaspCompiler.ANormalForms;
using ClaspCompiler.Data;
using ClaspCompiler.Semantics;

namespace ClaspCompiler.Common
{
    internal interface ILiteral : IPrintable, ISemExp, INormExp
    {
        public ITerm Value { get; }
    }
}
