namespace ClaspCompiler.SchemeCore.Abstract
{
    internal interface ICoreDef : IPrintable
    {
        public ICoreVar Variable { get; }
        public ICoreExp Value { get; }
    }
}
