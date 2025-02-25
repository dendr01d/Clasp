﻿using Clasp.Modules;

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
        internal override string DisplayDebug() => nameof(Nil);
    }

    internal sealed class Undefined : Atom
    {
        public static readonly Undefined Value = new Undefined();
        private Undefined() { }
        public override string ToString() => "#<undefined>";
        protected override string FormatType() => "Undefined";
        internal override string DisplayDebug() => nameof(Undefined);
    }

    internal sealed class VoidTerm : Atom
    {
        public static readonly VoidTerm Value = new VoidTerm();
        private VoidTerm() { }
        public override string ToString() => "#<void>";
        protected override string FormatType() => "Void";
        internal override string DisplayDebug() => nameof(VoidTerm);
    }

    internal sealed class ModuleTerm : Atom
    {
        public readonly Module Handle;

        public ModuleTerm(Module mdl) => Handle = mdl;
        public override string ToString() => string.Format("Mdl({0})", Handle.Name);
        protected override string FormatType() => "Module";
        internal override string DisplayDebug() => string.Format("Module '{0}'", Handle.Name);
    }
}
