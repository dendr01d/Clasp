using Clasp.Binding.Modules;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Binding.Environments
{
    internal sealed class ModuleEnv : LibraryEnv
    {
        public readonly string Name;
        public override LibraryEnv Predecessor { get; }
        public override Scope ImplicitScope { get; }

        public Module Module { get; private set; }

        public ModuleEnv(RuntimeEnv runtime, string name, Syntax stx) : base(runtime)
        {
            Name = name;
            Predecessor = runtime.Predecessor;
            ImplicitScope = new Scope(stx);
            Module = new FreshModule(this, stx);
        }

        public Module Visit(Processor pross)
        {

        }

        public CompiledModule Invoke(Processor pross)
        {

        }
    }
}
