using ClaspCompiler.IntermediateCLang.Abstract;
using ClaspCompiler.IntermediateLocLang.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeData.Abstract
{
    internal interface IAtom : ISchemeExp, ISemanticExp, INormArg
    { }
}
