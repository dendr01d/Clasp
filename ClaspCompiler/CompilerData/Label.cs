namespace ClaspCompiler.CompilerData
{
    internal sealed record Label
    {
        public string Name { get; init; }

        public Label(string name) => Name = name;

        public bool BreaksLines => false;
        public override string ToString() => string.Format("@{0}", Name);
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
