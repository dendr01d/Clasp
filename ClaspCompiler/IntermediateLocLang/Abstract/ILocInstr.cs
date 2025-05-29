namespace ClaspCompiler.IntermediateVarLang.Abstract
{
    internal interface ILocInstr : IPrintable
    {
        LocOp Operator { get; }
    }
}
