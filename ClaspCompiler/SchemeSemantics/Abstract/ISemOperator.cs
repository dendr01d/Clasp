namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemOperator : ISemLiteral
    {
        public string Name { get; init; }
    }
}
