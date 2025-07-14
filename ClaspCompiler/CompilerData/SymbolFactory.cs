using ClaspCompiler.SchemeData;
using ClaspCompiler.Text;

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

        private const string DEFAULT_PREFIX = "$";

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


        private static readonly char[] _superscript = [
            Unicode.Sup0,
            Unicode.Sup1,
            Unicode.Sup2,
            Unicode.Sup3,
            Unicode.Sup4,
            Unicode.Sup5,
            Unicode.Sup6,
            Unicode.Sup7,
            Unicode.Sup8,
            Unicode.Sup9
        ];

        private static string FormatGenName(string? name, uint id)
        {
            return string.Format("{0}{1}",
                name ?? DEFAULT_PREFIX, 
                new string([.. id.ToString().Select(c => _superscript[((int)c - (int)'0')])]));
        }

        public Symbol GenerateUnique(string? nameBasis = null)
        {
            uint counter = 1;
            string newName;

            do
            {
                newName = FormatGenName(nameBasis, counter++);
            }
            while (IsInterned(newName));

            return Intern(newName);
        }

        public Symbol GenerateUnique(Symbol sym) => GenerateUnique(sym.Name);
    }
}
