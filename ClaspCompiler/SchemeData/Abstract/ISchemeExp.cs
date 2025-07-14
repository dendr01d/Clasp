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
        public bool IsNil { get; }
        public bool IsFalse { get; }
    }

    internal static class ISchemeExpExtensions
    {
        public static SchemeData.Boolean AsBoolean(this ISchemeExp exp)
        {
            return exp.IsFalse ? SchemeData.Boolean.False : SchemeData.Boolean.True;
        }
    }
}
