using ClaspCompiler.IntermediateCLang.Abstract;
using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.IntermediateCLang
{
    internal sealed class ProgC0 : IPrintable
    {
        public Dictionary<Var, Type> LocalVariables { get; init; }
        public Dictionary<Label, ITail> LabeledTails { get; init; }

        public ProgC0(Dictionary<Var, Type> localVars, Dictionary<Label, ITail> labeledTails)
        {
            LocalVariables = localVars;
            LabeledTails = labeledTails;
        }

        public ProgC0(ITail entry)
            : this([], new Dictionary<Label, ITail>() { { new("start"), entry } })
        { }

        public bool CanBreak => true;

        public override string ToString()
        {
            return string.Format("(program {0} ({1}))",
                string.Join(' ', LocalVariables.Select(x => $"({x.Value.ToString().ToLower()} . {x.Key})")),
                string.Join(' ', LabeledTails.Select(x => $"({x.Key} . {x.Value}")));
        }

        public void Print(TextWriter writer, int indent)
        {
            writer.Write("(program "); // no hanging indent

            if (LocalVariables.Count == 0)
            {
                writer.Write("()");
            }

            writer.WriteLineIndent(indent);
            writer.WriteIndenting("  (", ref indent);

            if (LocalVariables.Count > 0)
            {
                writer.Write("({0} . ", LocalVariables.First().Value.ToString().ToLower());
                writer.Write(LocalVariables.First().Key, indent);
                writer.Write(')');

                foreach(var pair in LocalVariables.Skip(1))
                {
                    writer.WriteLineIndent(indent);
                    writer.Write("({0} . ", pair.Value.ToString().ToLower());
                    writer.Write(pair.Key, indent);
                    writer.Write(')');
                }

                writer.Write(')');
                writer.WriteLineIndent(indent - 1);
                writer.Write('(');
            }

            foreach (var pair in LabeledTails)
            {
                writer.Write('(');
                writer.Write(pair.Key);
                writer.WriteLineIndent(" .", indent);
                writer.Write('(');
                writer.Write(pair.Value, indent + 1);
                writer.Write("))");
            }

            writer.Write("))");
        }
    }
}
