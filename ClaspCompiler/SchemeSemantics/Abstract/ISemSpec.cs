namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemSpec : ISemExp
    {
        public SpecialKeyword Keyword { get; }
    }
}
