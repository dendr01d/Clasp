using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Modules
{
    internal sealed class CompiledModule : InstantiatedModule
    {
        public readonly RootEnv RuntimeEnvironment;

        private CompiledModule(string name, Scope outerEdge, Syntax stx, CoreForm form,
            Identifier[] exports)
            : base(name, outerEdge, exports)
        {
            ModuleForm = form;
            ParsedSyntax = stx;
        }

        public static CompiledModule InvokePartial(PartialModule partial)
        {

            // ???

            return new CompiledModule();
        }
    }
}
