using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.CompilerData
{
    /// <summary>
    /// Represents a semantic variable -- a memory location in which data is stored.
    /// </summary>
    internal sealed record Var : IPrintable, ISemExp, ICpsExp
    {
        public string Name { get; init; }

        public Var(string name) => Name = name;

        public bool BreaksLine => false;
        public string AsString => Name;
        public void Print(TextWriter writer, int indent) => writer.Write(Name);
        public sealed override string ToString() => Name;
    }
}
