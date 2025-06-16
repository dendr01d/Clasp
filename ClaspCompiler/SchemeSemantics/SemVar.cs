using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    /// <summary>
    /// Represents a semantic variable bound to some value in some context.
    /// </summary>
    internal class SemVar : ISemExp
    {
        public string Name { get; init; }
        public MetaData MetaData { get; init; }

        public SemVar(string name, MetaData? meta = null)
        {
            Name = name;
            MetaData = meta ?? new();
        }

        public bool BreaksLine => false;
        public string AsString => Name;
        public void Print(TextWriter writer, int indent) => writer.Write(Name);
        public sealed override string ToString() => Name;
    }
}
