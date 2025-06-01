using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed class ProgR1 : IPrintable
    {
        public string Info { get; private set; }
        public ISemanticExp Body { get; private set; }

        public ProgR1(string info, ISemanticExp body)
        {
            Info = info;
            Body = body;
        }

        public bool CanBreak => true;
        public override string ToString() => $"(program {Info} {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(program ", ref indent);
            writer.WriteLineIndent(Info.ToString(), indent);
            writer.Write(Body, indent);
            writer.Write(')');
        }
    }
}
