using Clasp.Binding.Modules;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Data.Terms
{
    /// <summary>
    /// Represents an irreducible value. 
    /// </summary>
    internal abstract class Atom : Term { }


    internal class Nil : Atom
    {
        public static readonly Nil Value = new Nil();
        protected Nil() { }
        public override string ToString() => "()";
        protected override string FormatType() => "Nil";

        public static implicit operator Syntax(Nil n) => Datum.NullSyntax();

        public static bool Is(Term? t) => t == Value || (t is Datum dat && dat.Expose() == Value);
    }

    internal sealed class Undefined : Atom
    {
        public static readonly Undefined Value = new Undefined();
        private Undefined() { }
        public override string ToString() => "#<undefined>";
        protected override string FormatType() => "Undefined";

        public static bool Is(Term? t) => t == Value || (t is Datum dat && dat.Expose() == Value);
    }

    internal sealed class VoidTerm : Atom
    {
        public static readonly VoidTerm Value = new VoidTerm();
        private VoidTerm() { }
        public override string ToString() => "#<void>";
        protected override string FormatType() => "Void";

        public static bool Is(Term? t) => t == Value || (t is Datum dat && dat.Expose() == Value);
    }

    internal sealed class ModuleHandle : Atom
    {
        public readonly Module Handle;
        public ModuleHandle(Module mdl) => Handle = mdl;
        public override string ToString() => string.Format("*{0}", Handle.Name);
        protected override string FormatType() => "ModuleHandle";
    }
}
