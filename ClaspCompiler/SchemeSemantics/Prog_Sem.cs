using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.SchemeTypes.TypeConstraints;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Prog_Sem(Body AbstractSyntaxTree) : IPrintable
    {
        public Dictionary<uint, SourceRef> SourceLookup { get; init; } = [];
        public Dictionary<SemVar, SchemeType> VariableTypes { get; init; } = [];
        public Dictionary<uint, SchemeType> NodeTypes { get; init; } = [];
        public List<TypeConstraint> TypeConstraints { get; init; } = [];
        public DisjointTypeSet TypeUnification { get; init; } = new();

        public IEnumerable<SemVar> VariablePool => VariableTypes.Keys;

        public bool BreaksLine => true;
        public string AsString => $"(program (...) {AbstractSyntaxTree})";
        public void Print(TextWriter writer, int indent)
        {
            writer.Write("(program");
            indent += 2;
            writer.WriteLineIndent(indent);

            if (TypeConstraints.Count > 0)
            {
                PrintTypeConstraints(writer, indent, TypeConstraints);
                writer.WriteLineIndent(indent);
            }

            if (TypeUnification.Count > 0)
            {
                writer.Write(TypeUnification, indent);
                writer.WriteLineIndent(indent);
            }

            writer.Write(AbstractSyntaxTree, indent);

            writer.Write(')');
        }
        public override string ToString() => AsString;

        private static void PrintTypeConstraints(TextWriter writer, int indent, IEnumerable<TypeConstraint> constraints)
        {
            writer.WriteIndenting("(where ", ref indent);
            writer.WriteLineByLine(constraints, indent);
            writer.Write(')');
        }
    }
}