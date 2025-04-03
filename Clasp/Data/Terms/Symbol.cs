using System;

using Clasp.Data.Static;

namespace Clasp.Data.Terms
{
    internal readonly struct Symbol : ITerm, IEquatable<Symbol>
    {
        public readonly string Name;

        private Symbol(string name) => Name = name;
        
        public bool Equals(Symbol other) => Name == other.Name;
        public bool Equals(ITerm? other) => other is Symbol sym && Equals(sym);
        public override bool Equals(object? other) => other is Symbol sym && Equals(sym);
        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name;

        public static Symbol Intern(string name)
        {
            if (InternedSymbols.TryGetSymbol(name, out Symbol result))
            {
                return result;
            }

            result = new Symbol(name);
            InternedSymbols.Intern(result);
            return result;
        }

        private const string GEN_PREFIX = "Γ";
        private const string DEFAULT_GEN = "gensym";

        public static Symbol GenSym() => GenSym(DEFAULT_GEN);

        public static Symbol GenSym(string name)
        {
            string newName = name;
            int counter = 0;

            while (InternedSymbols.Contains(newName))
            {
                newName = $"{name}{GEN_PREFIX}{counter++}";
            }

            return Intern(newName);
        }
    }
}
