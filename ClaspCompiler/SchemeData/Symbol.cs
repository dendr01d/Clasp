using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.SchemeData
{
    public sealed record Symbol : IAtom
    {
        private static Dictionary<string, Symbol> _interned = new();

        public readonly string Name;

        public bool IsAtom => true;
        public bool IsNil => false;

        private Symbol(string name) => Name = name;

        public static Symbol Intern(string name)
        {
            if (!_interned.ContainsKey(name))
            {
                _interned[name] = new Symbol(name);
            }
            return _interned[name];
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

        private const string _DEFAULT_NAME = "tmp";
        private static string FormatGenName(string? name, uint id)
        {
            return string.Format("{0}.{1}", name ?? _DEFAULT_NAME, id);
        }


        bool IPrintable.BreaksLine => false;
        public string AsString => Name;
        public void Print(TextWriter writer, int hanging = 0) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
