using ClaspCompiler.ANormalForms;
using ClaspCompiler.Data;
using ClaspCompiler.PseudoIl;
using ClaspCompiler.Semantics;

namespace ClaspCompiler.Common
{
    internal interface ILiteral : IPrintable,
        ISemExp,
        INormExp, INormArg,
        IArgument
    {
        public string GetTypeName();
        public ITerm GetValue();
    }

    internal static class ILiteralExtensions
    {
        public static string ToString(ILiteral lit) => lit.GetValue().ToString();
    }
}
