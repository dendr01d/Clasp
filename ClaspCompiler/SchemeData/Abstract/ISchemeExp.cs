using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeData.Abstract
{
    /// <summary>
    /// Any representable expression in the Scheme language
    /// </summary>
    internal interface ISchemeExp : IPrintable
    {
        public SchemeType Type { get; }
        public bool IsAtom { get; }
        bool IsNil { get; }
    }
}
