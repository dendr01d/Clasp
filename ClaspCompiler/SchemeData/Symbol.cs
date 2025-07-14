using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Symbol(string Name) : IAtom
    {
        public bool IsAtom => true;
        public bool IsNil => false;
        public bool IsFalse => false;
        public SchemeType Type => SchemeType.Symbol;

        bool IPrintable.BreaksLine => false;
        public string AsString => Name;
        public void Print(TextWriter writer, int hanging = 0) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
