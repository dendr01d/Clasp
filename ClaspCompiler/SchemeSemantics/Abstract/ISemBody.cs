namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemBody : ISemAstNode
    {
        public ISemDef[] Definitions { get; }
        public ISemCmd[] Commands { get; }
        public ISemExp Value { get; }
    }
}
