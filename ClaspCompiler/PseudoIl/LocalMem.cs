namespace ClaspCompiler.PseudoIl
{
    internal sealed class LocalMem : IMem, IArgument
    {
        public readonly int Index;
        public LocalMem(int index) => Index = index;
        public override string ToString() => $"L{Index}";
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
