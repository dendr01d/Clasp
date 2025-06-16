using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract.TypeConstraints;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed class Prog_Sem : IPrintable
    {
        public Dictionary<SemVar, SchemeType> VariableTypes { get; init; } = [];
        public HashSet<TypeConstraint> TypeConstraints { get; init; } = [];
        public ISemExp Body { get; private set; }

        public Prog_Sem(ISemExp body)
        {
            Body = body;
        }

        public bool BreaksLine => true;
        public string AsString => $"(program (...) {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(program ", ref indent);

            PrintVariableTypes(writer, indent);

            PrintTypeConstraints(writer, indent);

            writer.WriteLineIndent(indent);
            writer.Write(Body, indent);

            writer.Write(')');
        }
        public override string ToString() => AsString;

        private void PrintVariableTypes(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(with ", ref indent);
            writer.WriteLineByLine(VariableTypes, WriteVarType, indent);
            writer.Write(')');
        }

        private static void WriteVarType(TextWriter writer, KeyValuePair<SemVar, SchemeType> def, int indent)
        {
            writer.Write("[{0} . {1}]", def.Key, def.Value);
        }

        private void PrintTypeConstraints(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(where ", ref indent);
            writer.WriteLineByLine(TypeConstraints, WriteTypeConstraint, indent);
            writer.Write(')');
        }

        private static void WriteTypeConstraint(TextWriter writer, TypeConstraint constraint, int indent)
        {
            writer.Write(constraint, indent);
        }
    }
}
