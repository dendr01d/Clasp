
using System.Linq;

using Clasp.Binding;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.VirtualMachine;

namespace Clasp.Ops
{
    internal class SyntaxOps
    {
        public static Boolean BoundIdentifierEq(Identifier id1, Identifier id2) => id1.SameScopes(id2);
        public static Boolean FreeIdentifierEq(MachineState mx, Identifier id1, Identifier id2)
        {
            return id1.TryResolveBinding(mx.Phase, out RenameBinding? binding1)
                && id2.TryResolveBinding(mx.Phase, out RenameBinding? binding2)
                && binding1.BoundType == binding2.BoundType
                && binding1.Name == binding2.Name;
        }

        public static CharString SyntaxSource(Syntax stx) => new CharString(stx.Location.Source);
        public static IntegralNumeric SyntaxLine(Syntax stx) => new Integer(stx.Location.LineNumber);
        public static IntegralNumeric SyntaxColumn(Syntax stx) => new Integer(stx.Location.Column);
        public static IntegralNumeric SyntaxPosition(Syntax stx) => new Integer(stx.Location.StartingPosition);
        public static IntegralNumeric SyntaxSpan(Syntax stx) => new Integer(stx.Location.Length);
        public static Boolean SyntaxOriginal(Syntax stx) => stx.Location.Original;

        public static Term SyntaxE(Syntax stx) => stx.Expose();
        public static Syntax MakeSyntax(Term t, Syntax stx) => Syntax.WrapWithRef(t, stx);
    }
}
