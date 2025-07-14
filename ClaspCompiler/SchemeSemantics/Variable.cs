using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Variable(string Name, SourceRef Source) : VarBase(Name), ISemVar, IVisibleTypePredicate
    {
        public ISemVar Parameter => this;

    }
}