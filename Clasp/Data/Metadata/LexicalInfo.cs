using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Metadata
{
    internal sealed class LexicalInfo
    {
        public readonly Dictionary<int, Binding.ScopeMap> PhaseMaps;

        public LexicalInfo()
        {
            PhaseMaps = new Dictionary<int, Binding.ScopeMap>();
        }

        public LexicalInfo(LexicalInfo original)
        {
            PhaseMaps = new Dictionary<int, Binding.ScopeMap>(original.PhaseMaps);
        }
    }
}
