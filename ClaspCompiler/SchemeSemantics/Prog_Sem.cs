using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.SchemeTypes.TypeConstraints;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Prog_Sem(Body AbstractSyntaxTree) : IPrintable
    {
        public Dictionary<ISemVar, SchemeType> VariableTypes { get; init; } = [];
        public List<TypeConstraint> TypeConstraints { get; init; } = [];
        public SchemeType ProgramType { get; init; } = SchemeType.Any;

        public IEnumerable<ISemVar> VariablePool => VariableTypes.Keys;

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

            //if (TypeUnification.Count > 0)
            //{
            //    writer.Write(TypeUnification, indent);
            //    writer.WriteLineIndent(indent);
            //}

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