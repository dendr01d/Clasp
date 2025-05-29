namespace ClaspCompiler.IntermediateStackLang.Abstract
{
    internal interface IStackInstr : IPrintable
    {
        StackOp Operator { get; }
        IStackArg? Operand { get; }
    }
}
