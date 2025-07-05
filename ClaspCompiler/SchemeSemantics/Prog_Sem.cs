using System.Collections.Immutable;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Prog_Sem(Body AbstractSyntaxTree) : IPrintable
    {
        public ImmutableHashSet<SemVar> VariablePool { get; init; } = [];
        public Dictionary<uint, SourceRef> SourceLookup { get; init; } = [];

        public bool BreaksLine => true;
        public string AsString => $"(program (...) {AbstractSyntaxTree})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(program ", ref indent);

            writer.WriteLineIndent(indent);
            writer.Write(AbstractSyntaxTree, indent);

            writer.Write(')');
        }
        public override string ToString() => AsString;
    }
}