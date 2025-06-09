using ClaspCompiler.IntermediateCil.Abstract;

namespace ClaspCompiler.IntermediateCil
{
    internal sealed record RegLocal : IRegister
    {
        public int Index { get; init; }

        public RegLocal(int index) => Index = index;

        public bool BreaksLine => false;
        public string AsString => $"L_{Index}";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public override string ToString() => AsString;
    }
}
