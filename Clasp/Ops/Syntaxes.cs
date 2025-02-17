
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Ops
{
    internal class Syntaxes
    {
        public static CharString SyntaxSource(Syntax stx) => new CharString(stx.LexContext.Location.Source);
        public static IntegralNumeric SyntaxLine(Syntax stx) => new Integer(stx.LexContext.Location.LineNumber);
        public static IntegralNumeric SyntaxColumn(Syntax stx) => new Integer(stx.LexContext.Location.Column);
        public static IntegralNumeric SyntaxPosition(Syntax stx) => new Integer(stx.LexContext.Location.StartingPosition);
        public static IntegralNumeric SyntaxSpan(Syntax stx) => new Integer(stx.LexContext.Location.Length);
        public static Boolean SyntaxOriginal(Syntax stx) => stx.LexContext.Location.Original;

        public static Term SyntaxE(Syntax stx) => stx.Expose();

        public static Term SyntaxToList(Syntax stx) => stx is SyntaxList stl ? stl.Expose() : Boolean.False;
        public static Term SyntaxToDatum(Syntax stx) => stx.ToDatum();
        public static Syntax DatumToSyntax(Syntax copy, Term t) => Syntax.FromDatum(t, copy);
    }
}
