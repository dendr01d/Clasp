using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.IntermediateVarLang
{
    internal sealed class ProgLoc0
    {
        public Dictionary<Var, HashSet<Var>>? Conflicts { get; init; }
        public Dictionary<Var, SchemeType>? LocalVariables { get; init; }
        public readonly Dictionary<Label, Block> LabeledBlocks;

        public ProgLoc0(Dictionary<Label, Block> labeledBlocks)
            : this(null, labeledBlocks)
        { }

        public ProgLoc0(Dictionary<Var, SchemeType>? localVars, Dictionary<Label, Block> labeledBlocks)
            : this(null, localVars, labeledBlocks)
        { }

        public ProgLoc0(
            Dictionary<Var, HashSet<Var>>? conflicts,
            Dictionary<Var, SchemeType>? localVars,
            Dictionary<Label, Block> labeledBlocks)
        {
            Conflicts = conflicts;
            LocalVariables = localVars;
            LabeledBlocks = labeledBlocks;
        }

        public override string ToString()
        {
            return string.Format("(program (...) ({0}))",
                string.Join(' ', LabeledBlocks.Select(x => string.Format("({0} . {1})", x.Key, x.Value))));
        }

        public void Print(TextWriter writer, int indent)
        {
            writer.Write("(program ");

            writer.WriteLineIndent(indent);
            PrintLocalVariables(writer, LocalVariables, indent);

            writer.WriteLineIndent(indent);
            PrintConflicts(writer, Conflicts, indent);

            writer.WriteLineIndent(indent);
            writer.WriteIndenting("  (", ref indent);

            foreach (var pair in LabeledBlocks)
            {
                writer.Write('(');
                writer.Write(pair.Key);
                writer.WriteLineIndent(" .", indent);
                writer.Write(pair.Value, indent);
                writer.Write(')');
            }

            writer.Write("))");
        }

        private static void PrintLocalVariables(TextWriter writer, Dictionary<Var, SchemeType>? localVars, int indent)
        {
            writer.WriteIndenting("  (", ref indent);

            if (localVars is not null)
            {
                writer.Write("({0} . ", localVars.First().Key);
                writer.Write(string.Format("{{{0}}}", string.Join(", ", localVars.First().Value)));
                writer.Write(')');

                foreach (var pair in localVars.Skip(1))
                {
                    writer.WriteLineIndent(indent);
                    writer.Write("({0} . ", pair.Key);
                    writer.Write(string.Format("{{{0}}}", string.Join(", ", pair.Value)));
                    writer.Write(')');
                }
            }

            writer.Write(')');
        }

        private static void PrintConflicts(TextWriter writer, Dictionary<Var, HashSet<Var>>? conflicts, int indent)
        {
            writer.WriteIndenting("  (", ref indent);

            if (conflicts is not null)
            {
                writer.Write("({0} . ", conflicts.First().Key);
                writer.Write(string.Format("{{{0}}}", string.Join(", ", conflicts.First().Value)));
                writer.Write(')');

                foreach (var pair in conflicts.Skip(1))
                {
                    writer.WriteLineIndent(indent);
                    writer.Write("({0} . ", pair.Key);
                    writer.Write(string.Format("{{{0}}}", string.Join(", ", pair.Value)));
                    writer.Write(')');
                }
            }

            writer.Write(')');
        }
    }
}
