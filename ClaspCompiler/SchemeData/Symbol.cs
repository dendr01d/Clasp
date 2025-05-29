using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Symbol : ISchemeExp, IEquatable<Symbol>
    {
        public readonly string Name;

        public bool IsAtom => true;
        public bool IsNil => false;

        public Symbol(string name) => Name = name;

        public bool CanBreak => false;
        public override string ToString() => Name;
        public void Print(TextWriter writer, int indent) => writer.Write(Name);
    }
}
