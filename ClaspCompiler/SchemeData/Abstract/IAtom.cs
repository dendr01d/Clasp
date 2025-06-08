using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeData.Abstract
{
    /// <summary>
    /// A scheme datum of irreducible complexity
    /// </summary>
    internal interface IAtom : ISchemeExp, ISemExp, ICpsArg
    { }
}
