using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding;
using Clasp.Binding.Modules;
using Clasp.Exceptions;

namespace Clasp.Data.Static
{
    internal static class ModuleCache
    {
        private static Dictionary<string, Module> _cache = new Dictionary<string, Module>();

        public static Scope GetScope(string moduleName)
        {
            if (!TryGet(moduleName, out Module? mdl)
                || mdl is DeclaredModule)
            {
                Module.Visit(moduleName);
                return GetScope(moduleName);
            }
            else if (mdl is ParsedModule pMdl)
            {
                return pMdl.ExportedScope;
            }

            throw new ClaspGeneralException("Impossible module state?");
        }

        public static bool Contains(string moduleName) => _cache.ContainsKey(moduleName);

        public static Module Get(string moduleName) => _cache[moduleName];

        public static bool TryGet(string moduleName, [NotNullWhen(true)] out Module? mdl)
        {
            return _cache.TryGetValue(moduleName, out mdl);
        }

        public static void Update(Module mdl)
        {
            _cache[mdl.Name] = mdl;
        }
    }
}
