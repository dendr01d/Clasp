using ClaspCompiler.IntermediateCil.Abstract;

namespace ClaspCompiler.IntermediateCil
{
    internal sealed record RegParam : IRegister
    {
        public int Index { get; init; }

        public RegParam(int index) => Index = index;

        public bool BreaksLine => false;
        public string AsString => $"A_{Index}";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public override string ToString() => AsString;
    }
}
