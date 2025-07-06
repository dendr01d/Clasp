using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Lambda(ISemParameters? Parameters, Body Body, SourceRef Source) : ISemExp
    {
        public bool BreaksLine => Body.BreaksLine;
        public string AsString => $"(lambda {Parameters} {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting($"(lambda ", ref indent);
            writer.Write(FormatParameters(Parameters));

            if (BreaksLine)
            {
                writer.WriteLineIndent(indent);
            }
            else
            {
                writer.Write(' ');
            }
            writer.Write(Body, indent);

            writer.Write(')');
        }
        public sealed override string ToString() => AsString;

        private static string FormatParameters(ISemParameters? parms) => parms switch
        {
            ISemVar sv => sv.ToString(),
            FormalParameters fp => $"({fp.Parameter}{FormatRemainingParameters(fp.Next)})",
            _ => "()"
        };
        private static string FormatRemainingParameters(ISemParameters? parms) => parms switch
        {
            ISemVar sv => $" . {sv}",
            FormalParameters fp => $" {fp.Parameter}{FormatRemainingParameters(fp.Next)}",
            _ => string.Empty
        };
    }
}
