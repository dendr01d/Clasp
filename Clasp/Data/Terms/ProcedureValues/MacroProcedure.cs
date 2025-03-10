using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;

namespace Clasp.Data.Terms.Procedures
{

    internal sealed class MacroProcedure : CompoundProcedure
    {
        public MacroProcedure(string parameter, MutableEnv enclosing, Sequential body)
            : base([parameter], [], enclosing, body)
        { }

        public override string ToString() => string.Format("#<macro({0})>", Parameters[0]);

        protected override string FormatType() => "Macro";
    }
}
