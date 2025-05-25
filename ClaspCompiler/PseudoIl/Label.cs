namespace ClaspCompiler.PseudoIl
{
    internal sealed class Label : IMem, IArgument
    {
        public readonly string Name;
        public Label(string name) => Name = name;
        public override string ToString() => Name;
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
