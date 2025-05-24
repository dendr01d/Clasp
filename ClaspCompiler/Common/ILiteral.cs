using ClaspCompiler.ANormalForms;
using ClaspCompiler.Data;
using ClaspCompiler.Semantics;

namespace ClaspCompiler.Common
{
    internal interface ILiteral : IPrintable,
        ISemExp,
        INormExp, INormArg
    {
        public ITerm GetValue();
    }

    internal static class ILiteralExtensions
    {
        public static string ToString(ILiteral lit) => lit.GetValue().ToString();
    }
}
