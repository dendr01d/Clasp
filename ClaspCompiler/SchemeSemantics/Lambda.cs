using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Lambda(ISemVar[] Parameters, ISemVar? DottedParameter, Body Body, SourceRef Source) : ISemExp
    {
        public bool BreaksLine => Body.BreaksLine;
        public string AsString => $"(lambda {Parameters} {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting($"(lambda ", ref indent);
            writer.Write(FormatParameters(Parameters, DottedParameter));

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

        private static string FormatParameters(ISemVar[] parameters, ISemVar? dottedParam)
        {
            if (dottedParam is null)
            {
                if (parameters.Length > 0)
                {
                    return $"({string.Join(' ', parameters.AsEnumerable())})";
                }
                else
                {
                    return "()";
                }
            }
            else
            {
                if (parameters.Length == 0)
                {
                    return dottedParam.ToString();
                }
                else
                {
                    return $"({string.Join(' ', parameters.AsEnumerable())} . {dottedParam})";
                }
            }
        }

        //private static string FormatParameters(ISemParameters? parms) => parms switch
        //{
        //    ISemVar sv => sv.ToString(),
        //    FormalParameters fp => $"({fp.Parameter}{FormatRemainingParameters(fp.Next)})",
        //    _ => "()"
        //};
        //private static string FormatRemainingParameters(ISemParameters? parms) => parms switch
        //{
        //    ISemVar sv => $" . {sv}",
        //    FormalParameters fp => $" {fp.Parameter}{FormatRemainingParameters(fp.Next)}",
        //    _ => string.Empty
        //};
    }
}