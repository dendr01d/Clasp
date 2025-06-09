using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Nil : IAtom
    {
        public static readonly Nil Instance = new();

        public bool IsAtom => true;
        public bool IsNil => true;

        private Nil() { }

        bool IPrintable.BreaksLine => false;
        public string AsString => "()";
        public void Print(TextWriter writer, int hanging = 0) => writer.Write(AsString);
        public sealed override string ToString() => AsString;

        public bool Equals(ICpsExp? other) => other is Nil;
    }
}
