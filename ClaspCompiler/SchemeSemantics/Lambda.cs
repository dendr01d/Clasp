using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Lambda(ParamsForm Parameters, BodyForm Body) : ISemExp
    {
        public bool BreaksLine => Body.BreaksLine;
        public string AsString => $"(lambda {Parameters.AsStandalone} {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting($"(lambda ", ref indent);
            Parameters.PrintStandalone(writer, indent);

            if (Body.BreaksLine)
            {
                writer.WriteLineIndent(indent);
            }
            else
            {
                writer.Write(' ');
            }

            writer.Write(Body);
            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}
