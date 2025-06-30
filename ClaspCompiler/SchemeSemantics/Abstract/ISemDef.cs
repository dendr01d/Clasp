namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemDef : ISemAstNode
    {
        public ISemVar Variable { get; }
        public ISemExp Value { get; }
    }
}
