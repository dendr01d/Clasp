using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Lambda(ISemFormals Formals, ISemBody Body, uint AstId) : ISemExp
    {
        public bool BreaksLine => Body.BreaksLine;
        public string AsString => $"(lambda {Formals} {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting($"(lambda ", ref indent);
            writer.Write(Formals, indent);

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
    }
}
