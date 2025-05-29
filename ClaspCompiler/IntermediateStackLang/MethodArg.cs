using ClaspCompiler.IntermediateStackLang.Abstract;

namespace ClaspCompiler.IntermediateStackLang
{
    internal sealed record MethodArg : IRegister
    {
        public int Index { get; }
        public MethodArg(int index) => Index = index;
        public bool CanBreak => false;
        public override string ToString() => $"A{Index}";
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}