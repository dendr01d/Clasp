namespace ClaspCompiler.IntermediateStackLang.Abstract
{
    internal enum StackOp
    {
        Dupe,
        Add, Sub,
        Neg,
        Load, Store, Pop,
        Call,
        Return,
        Jump
    }
}
