using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;

using Clasp.Data.Static;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.Data.Terms
{
    internal class Symbol : Atom
    {
        public readonly string Name;
        protected Symbol(string name)
        {
            Name = name;
            _interned.Add(name, this);
        }

        private static readonly Dictionary<string, Symbol> _interned = [];
        protected static bool IsInterned(string name) => _interned.ContainsKey(name);

        public static Symbol Intern(string name)
        {
            if (!_interned.ContainsKey(name))
            {
                _interned[name] = new Symbol(name);
            }
            return _interned[name];
        }

        public override string ToString() => Name;
        protected override string FormatType() => "Symbol";
    }

    internal class GenSym : Symbol
    {
        // Gamma, for "GenSym"
        private const string _SEP = "-Γ";

        private static string GenerateUniqueName(string partial)
        {
            string output = partial;
            uint counter = 1;

            while (IsInterned(output))
            {
                output = string.Format("{0}{1}{2}", partial, _SEP, counter++);
            }
            return output;
        }

        public GenSym(string fromName) : base(GenerateUniqueName(fromName)) { }
        public GenSym() : this("GenSym") { }

        protected override string FormatType() => "GenSym";
    }

    /// <summary>
    /// Special symbols that cannot be linguistically represented. They act as
    /// "unshadowable" representations of certain important keywords by dint of being
    /// an entirely different Type.
    /// </summary>
    internal sealed class ReservedSymbol : Symbol
    {
        public ReservedSymbol(string name) : base(name) { }
        protected override string FormatType() => "ReservedSymbol";
    }
}
