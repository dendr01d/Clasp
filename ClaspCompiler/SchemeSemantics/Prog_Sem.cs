using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed class Prog_Sem : IPrintable
    {
        public Dictionary<Var, SchemeType> VariableTypes { get; init; }
        public ISemExp Body { get; private set; }

        public Prog_Sem(Dictionary<Var, SchemeType> varTypes, ISemExp body)
        {
            VariableTypes = varTypes;
            Body = body;
        }

        public Prog_Sem(ISemExp body) : this([], body) { }

        public bool BreaksLine => true;
        public string AsString => $"(program (...) {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(program ", ref indent);

            writer.Write('(');
            writer.WriteLineByLine(VariableTypes, WriteVarType, indent + 1);
            writer.Write(')');

            writer.WriteLineIndent(indent);
            writer.Write(Body, indent);

            writer.Write(')');
        }
        public override string ToString() => AsString;

        private static void WriteVarType(TextWriter writer, KeyValuePair<Var, SchemeType> def, int indent)
        {
            writer.Write("[{0} . {1}]", def.Key, def.Value);
        }
    }
}
