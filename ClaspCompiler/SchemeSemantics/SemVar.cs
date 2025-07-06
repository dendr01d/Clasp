using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record SemVar(string Name, SourceRef Source) : VarBase(Name), ISemExp, ISemParameters
    {
        public SemVar Parameter => this;
        public ISemParameters? Next => null;

    }
}
