
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;

namespace Clasp.Data.Metadata
{
    /// <summary>
    /// Manages state information of a program expansion either ongoing or completed.
    /// </summary>
    internal class ParseContext
    {
        /// <summary>
        /// Records compile-time bindings of identifiers in the current lexical scope.
        /// Closures are used to ensure the locality of bound values.
        /// </summary>
        public readonly Dictionary<int, Closure> CompileTimeEnvironments;

        private readonly RootEnv _rootEnv;
    
        public ParseContext(RootEnv env)
        {
            CompileTimeEnvironments = new Dictionary<int, Closure>();
            _rootEnv = env;
        }

        protected ParseContext(ParseContext pCtx)
        {
            CompileTimeEnvironments = pCtx.CompileTimeEnvironments;
            _rootEnv = pCtx._rootEnv;
        }

        public Closure EnvByPhase(int phase)
        {
            if (!CompileTimeEnvironments.ContainsKey(phase))
            {
                CompileTimeEnvironments[phase] = _rootEnv.Enclose();
            }
            return CompileTimeEnvironments[phase];
        }

        public bool TryLookupMacro(int phase, CompileTimeBinding binding,
            [NotNullWhen(true)] out MacroProcedure? macro)
        {
            if (binding.BoundType == BindingType.Transformer
                && EnvByPhase(phase).TryGetValue(binding.Name, out Term? maybeMacro)
                && maybeMacro is MacroProcedure definitelyMacro)
            {
                macro = definitelyMacro;
                return true;
            }

            macro = null;
            return false;
        }
    }
}
