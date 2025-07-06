namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemParameters : ISemSubForm
    {
        ISemVar Parameter { get; }
        ISemParameters? Next { get; }
    }
}
