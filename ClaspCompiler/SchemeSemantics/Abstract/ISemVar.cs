namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemVar : ISemExp
    {
        string Name { get; }
    }
}