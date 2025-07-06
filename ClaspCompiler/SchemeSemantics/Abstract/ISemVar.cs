namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemVar : ISemExp, ISemParameters
    {
        string Name { get; }
    }
}
