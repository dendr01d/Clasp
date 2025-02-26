using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding.Environments;
using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.VirtualMachine;
using Clasp.Process;

namespace Clasp.Modules
{
    class InterpretedModule : ProcessedModule
    {
        private readonly RootEnv _env;

        private InterpretedModule(string name, Identifier[] ids, RootEnv interpretedEnv) : base(name, ids)
        {
            _env = interpretedEnv;
        }

        public bool TryLookup(string key, out Term? def)
        {
            return _env.TryGetValue(key, out def);
        }

        public static InterpretedModule Interpret(ParsedModule pm)
        {
            MachineState state = new MachineState(pm.ModuleBody, new RootEnv());
            MachineState final = Interpreter.InterpretToCompletion(state);

            return new InterpretedModule(pm.Name, pm.ExportedIds, final.CurrentEnv.Root);
        }
    }
}
