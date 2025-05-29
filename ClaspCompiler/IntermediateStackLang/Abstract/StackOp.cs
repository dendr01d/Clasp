namespace ClaspCompiler.IntermediateStackLang.Abstract
{
    internal enum StackOp
    {
        Add, Sub,
        Neg,
        Load, Store, Pop,
        Call,
        Return,
        Jump
    }
}
