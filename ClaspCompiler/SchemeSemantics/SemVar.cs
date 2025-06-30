using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record SemVar(string Name, uint AstId) : VarBase(Name), ISemVar
    { }
}
