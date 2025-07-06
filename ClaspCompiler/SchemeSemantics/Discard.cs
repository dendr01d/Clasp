using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    /// <summary>
    /// Represents parsed syntax that is meant to be discarded -- macro forms and such that won't be
    /// included in the final program.
    /// </summary>
    internal sealed record Discard(SourceRef Source) : ISemForm
    {
        public bool BreaksLine => false;
        public string AsString => "(Discarded)";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
