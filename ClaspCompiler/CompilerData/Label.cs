using ClaspCompiler.IntermediateCil.Abstract;

namespace ClaspCompiler.CompilerData
{
    /// <summary>
    /// Represents an indexed location within a collection of sequential executable instructions.
    /// </summary>
    internal sealed record Label : IPrintable, ICilArg
    {
        public string Name { get; init; }

        public Label(string name) => Name = name;

        public bool BreaksLine => false;
        public string AsString => Name;
        public void Print(TextWriter writer, int indent) => writer.Write(Name);
        public sealed override string ToString() => Name;
    }
}
