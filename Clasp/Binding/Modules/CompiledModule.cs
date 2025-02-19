using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Binding.Modules
{
    internal sealed class CompiledModule : Module
    {
        public readonly CoreForm ModuleForm;
        public readonly Syntax ParsedSyntax;
        public readonly string[] ExportedNames;

        private CompiledModule(ModuleEnv env, Syntax stx, CoreForm form, string[] names) : base(env)
        {
            ModuleForm = form;
            ParsedSyntax = stx;
            ExportedNames = names;
        }

        public static CompiledModule InvokePartial(PartialModule partial)
        {

            // ???

            return new CompiledModule();
        }
    }
}
