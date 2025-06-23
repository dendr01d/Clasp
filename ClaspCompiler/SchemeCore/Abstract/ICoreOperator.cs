namespace ClaspCompiler.SchemeCore.Abstract
{
    internal interface ICoreOperator : ICoreLiteral
    {
        public string Name { get; init; }
        public bool SideEffective { get; init; }
    }

}
