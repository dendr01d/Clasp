using ClaspCompiler.SchemeData;

namespace ClaspCompiler.CompilerData
{
    /// <summary>
    /// A factory for interning and creating unique <see cref="Symbol"/> objects.
    /// Symbols may be interned globally or locally per instance of this class.
    /// </summary>
    internal sealed class SymbolFactory
    {
        private static readonly Dictionary<string, Symbol> _globalInternment = [];
        private readonly Dictionary<string, Symbol> _localInternment;

        private const string DEFAULT_PREFIX = "σ";

        public SymbolFactory()
        {
            _localInternment = [];
        }

        /// <summary>
        /// Return the singular <see cref="Symbol"/> with the given name, either
        /// at the global scale or only within this local instance.
        /// </summary>
        public Symbol Intern(string name)
        {
            if (_globalInternment.TryGetValue(name, out Symbol? globalValue))
            {
                return globalValue;
            }
            
            if (!_localInternment.TryGetValue(name, out Symbol? value))
            {
                value = new Symbol(name);
                _localInternment[name] = value;
            }

            return value;
        }

        /// <summary>
        /// Return the singular global <see cref="Symbol"/> with the given name.
        /// </summary>
        public static Symbol InternGlobal(string name)
        {
            if (!_globalInternment.TryGetValue(name, out Symbol? value))
            {
                value = new Symbol(name);
                _globalInternment[name] = value;
            }

            return value;
        }

        private bool IsInterned(string name) => _globalInternment.ContainsKey(name) || _localInternment.ContainsKey(name);

        private static string FormatGenName(string? name, uint id)
        {
            return string.Format("{0}{1}", name ?? DEFAULT_PREFIX, id);
        }

        public Symbol GenSym(string? nameBasis = null)
        {
            if (nameBasis is not null && IsInterned(nameBasis))
            {
                return Intern(nameBasis);
            }

            uint counter = 1;
            string newName;

            do
            {
                newName = FormatGenName(nameBasis, counter++);
            }
            while (IsInterned(newName));

            return Intern(newName);
        }

        public Symbol GenSym(Symbol sym) => GenSym(sym.Name);
    }
}
