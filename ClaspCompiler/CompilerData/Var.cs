using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.CompilerData
{
    /// <summary>
    /// Represents a semantic variable -- a memory location in which data is stored.
    /// </summary>
    internal sealed record Var : IPrintable, ISemAtm, ICpsArg
    {
        public string Name { get; init; }

        public Var(string name) => Name = name;

        public bool BreaksLine => false;
        public string AsString => Name;
        public void Print(TextWriter writer, int indent) => writer.Write(Name);
        public sealed override string ToString() => Name;

        public bool Equals(ICpsExp? other) => other is Var v && Name == v.Name;
    }
}
