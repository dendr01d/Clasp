namespace ClaspCompiler.IntermediateStackLang.Abstract
{
    internal interface IRegister : IStackArg
    {
        int Index { get; }
    }
}
