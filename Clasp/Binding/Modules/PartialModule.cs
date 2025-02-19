using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding.Environments;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Binding.Modules
{
    /// <summary>
    /// The partially-expanded syntax of an imported module form.
    /// </summary>
    internal sealed class PartialModule : Module
    {
        public readonly Syntax PartiallyExpandedSyntax;
        public readonly string[] ExportedNames;

        public readonly List<Closure> CompileTimeEnvs;

        private PartialModule(ModuleEnv env, Syntax stx, string[] exportedNames) : base(env)
        {
            CompileTimeEnvs = new List<Closure>();
        }

        public static PartialModule VisitFresh(FreshModule fresh)
        {

            //???
            return new PartialModule();
        }
    }
}
