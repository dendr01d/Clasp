using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding.Environments;
using Clasp.Data.Terms;

namespace Clasp.Binding
{
    internal sealed class Module
    {
        public readonly string Name;
        public readonly string[] ExportedNames;

        private RootEnv _env;

        public Module(string name, RootEnv env, string[] exports)
        {
            Name = name;
            _env = env;
            ExportedNames = exports;
        }

        public bool TryLookup(string key, [NotNullWhen(true)] out Term? def)
        {
            def = null;
            return ExportedNames.Contains(key) && _env.TryGetValue(key, out def);
        }
    }
}
