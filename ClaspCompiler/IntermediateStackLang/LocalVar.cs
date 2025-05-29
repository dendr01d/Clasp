using ClaspCompiler.IntermediateStackLang.Abstract;

namespace ClaspCompiler.IntermediateStackLang
{
    internal sealed record LocalVar : IRegister
    {
        public int Index { get; }
        public LocalVar(int index) => Index = index;
        public bool CanBreak => false;
        public override string ToString() => $"L{Index}";
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
