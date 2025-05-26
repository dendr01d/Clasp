using ClaspCompiler.ANormalForms;
using ClaspCompiler.Data;
using ClaspCompiler.PseudoIl;
using ClaspCompiler.Semantics;

namespace ClaspCompiler.Common
{
    internal interface IAtom : IPrintable,
        ISemExp,
        INormExp, INormArg,
        IArgument
    {
        public TypeName TypeName { get; }
    }

    internal interface IAtom<out T> : IAtom
        where T : ITerm
    {
        public T Data { get; }
    }
}
