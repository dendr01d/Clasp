using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemForm : ISemAstNode
    {
        SourceRef Source { get; }
    }
}