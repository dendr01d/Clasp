
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.Terms;

namespace Clasp.Data.Metadata
{
    internal class ParseContext
    {
        /// <summary>
        /// Records compile-time bindings of identifiers in the current lexical scope. Closures
        /// are used to ensure the locality of bound values.
        /// </summary>
        public readonly Environment CompileTimeEnv;

        /// <summary>The current phase of expansion.</summary>
        public readonly int Phase;
    
        public ParseContext(Environment env, int phase)
        {
            CompileTimeEnv = env;
            Phase = phase;
        }

        public bool TryLookupMacro(CompileTimeBinding binding,
            [NotNullWhen(true)] out MacroProcedure? macro)
        {
            if (binding.BoundType == BindingType.Transformer
                && CompileTimeEnv.TryGetValue(binding.Name, out Term? maybeMacro)
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
