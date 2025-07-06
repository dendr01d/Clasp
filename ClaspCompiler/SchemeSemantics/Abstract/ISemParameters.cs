namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemParameters : ISemSubForm
    {
        SemVar Parameter { get; }
        ISemParameters? Next { get; }
    }
}
