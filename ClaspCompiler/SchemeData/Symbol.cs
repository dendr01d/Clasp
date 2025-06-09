using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Symbol : IAtom
    {
        private static Dictionary<string, Symbol> _interned = new();

        public readonly string Name;

        public bool IsAtom => true;
        public bool IsNil => false;

        private Symbol(string name) => Name = name;

        public static Symbol Intern(string name)
        {
            if (_interned.TryGetValue(name, out Symbol? sym))
            {
                return sym;
            }
            else
            {
                Symbol output = new Symbol(name);
                _interned[name] = output;
                return output;
            }
        }

        public static void ResetInterment() => _interned = new();

        public static Symbol GenSym(string? insp = null)
        {
            if (insp is not null && !_interned.ContainsKey(insp))
            {
                return Intern(insp);
            }

            uint counter = 1;
            string newName;

            do
            {
                newName = FormatGenName(insp, counter++);
            }
            while (_interned.ContainsKey(newName));

            return Intern(newName);
        }

        private const string _defaultName = "tmp";
        private static string FormatGenName(string? name, uint id)
        {
            return string.Format("{0}.{1}", name ?? _defaultName, id);
        }


        bool IPrintable.BreaksLine => false;
        public string AsString => Name;
        public void Print(TextWriter writer, int hanging = 0) => writer.Write(AsString);
        public sealed override string ToString() => AsString;

        public bool Equals(ICpsExp? other) => other is Symbol sym && sym.Name == Name;
    }
}
