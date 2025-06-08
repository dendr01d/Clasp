namespace ClaspCompiler.SchemeData.Abstract
{
    /// <summary>
    /// Any representable expression in the Scheme language
    /// </summary>
    internal interface ISchemeExp : IPrintable
    {
        bool IsAtom { get; }
        bool IsNil { get; }
    }
}
