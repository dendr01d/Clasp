namespace ClaspCompiler.Data
{
    internal interface ITerm : IPrintable
    {
        bool IsAtom { get; }
        bool IsNil { get; }
    }
}
