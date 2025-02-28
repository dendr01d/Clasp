
using Clasp.Binding;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.VirtualMachine;

namespace Clasp.Ops
{
    internal class SyntaxOps
    {
        public static Boolean BoundIdentifierEq(Identifier id1, Identifier id2) => id1.LexContext.SameScopes(id2.LexContext);
        public static Boolean FreeIdentifierEq(MachineState mx, Identifier id1, Identifier id2)
        {
            return id1.TryResolveBinding(mx.Phase, out ExpansionVarNameBinding? binding1)
                && id2.TryResolveBinding(mx.Phase, out ExpansionVarNameBinding? binding2)
                && binding1.BoundType == binding2.BoundType
                && binding1.Name == binding2.Name;
        }

        public static CharString SyntaxSource(Syntax stx) => new CharString(stx.LexContext.Location.Source);
        public static IntegralNumeric SyntaxLine(Syntax stx) => new Integer(stx.LexContext.Location.LineNumber);
        public static IntegralNumeric SyntaxColumn(Syntax stx) => new Integer(stx.LexContext.Location.Column);
        public static IntegralNumeric SyntaxPosition(Syntax stx) => new Integer(stx.LexContext.Location.StartingPosition);
        public static IntegralNumeric SyntaxSpan(Syntax stx) => new Integer(stx.LexContext.Location.Length);
        public static Boolean SyntaxOriginal(Syntax stx) => stx.LexContext.Location.Original;

        public static Term SyntaxE(Syntax stx) => stx.Expose();
        public static Syntax MakeSyntax(Term t, Syntax stx) => Syntax.FromDatum(t, stx.LexContext);

        public static Term SyntaxToList(Syntax stx) => stx is SyntaxList stl ? stl.Expose() : Boolean.False;
        public static Term SyntaxToDatum(Syntax stx) => stx.ToDatum();
        public static Syntax DatumToSyntax(Syntax copy, Term t) => Syntax.FromDatum(t, copy);
    }
}
