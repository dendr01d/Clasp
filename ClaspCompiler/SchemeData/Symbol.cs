using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Symbol : IAtom
    {
        private static Dictionary<string, Symbol> _interned = new();

        public readonly string Name;

        public SchemeType Type => AtomicType.Symbol;
        public bool IsAtom => true;
        public bool IsNil => false;

        public Symbol(string name) => Name = name;

        bool IPrintable.BreaksLine => false;
        public string AsString => Name;
        public void Print(TextWriter writer, int hanging = 0) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
