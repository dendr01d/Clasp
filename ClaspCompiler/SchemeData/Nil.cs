using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Nil : IAtom
    {
        public static readonly Nil Instance = new();

        public SchemeType Type => AtomicType.Nil;
        public bool IsAtom => true;
        public bool IsNil => true;
        public bool IsFalse => false;

        private Nil() { }

        public bool BreaksLine => false;
        public string AsString => "()";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
