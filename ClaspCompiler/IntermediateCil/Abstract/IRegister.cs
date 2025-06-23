namespace ClaspCompiler.IntermediateCil.Abstract
{
    internal interface IRegister : ICilArg
    {
        int Index { get; }
    }
}
