namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemApp : ISemExp
    {
        public ISemExp[] Arguments { get; }
    }
}
