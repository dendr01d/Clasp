using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed class Prog_Sem : IPrintable
    {
        public ISemTop[] TopLevelForms { get; private set; }

        public Prog_Sem(ISemTop[] topLevelForms)
        {
            TopLevelForms = topLevelForms;
        }

        public bool BreaksLine => true;
        public string AsString => $"(program (...) {TopLevelForms})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(program ", ref indent);

            //writer.Write('(');
            //writer.WriteLineByLine(VariableTypes, WriteVarType, indent + 1);
            //writer.Write(')');

            writer.WriteLineIndent(indent);
            writer.WriteLineByLine(TopLevelForms, indent);

            writer.Write(')');
        }
        public override string ToString() => AsString;

        private static void WriteVarType(TextWriter writer, KeyValuePair<Var, SchemeType> def, int indent)
        {
            writer.Write("[{0} . {1}]", def.Key, def.Value);
        }
    }
}