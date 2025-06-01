using ClaspCompiler.IntermediateLocLang.Abstract;
using ClaspCompiler.IntermediateStackLang.Abstract;

namespace ClaspCompiler.SchemeData.Abstract
{
    internal interface IValue : IAtom, ILocArg, IStackArg { }
}
