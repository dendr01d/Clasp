namespace ClaspCompiler.SchemeData.Abstract
{
    internal interface ISchemeExp : IPrintable
    {
        bool IsAtom { get; }
        bool IsNil { get; }
    }
}
