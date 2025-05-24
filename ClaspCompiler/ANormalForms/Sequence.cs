namespace ClaspCompiler.ANormalForms
{
    internal sealed class Sequence : ITail
    {
        public readonly IStatement Statement;
        public readonly ITail Tail;

        public Sequence(IStatement stmt, ITail tail)
        {
            Statement = stmt;
            Tail = tail;
        }

        public override string ToString() => $"(seq {Statement} {Tail})";
        public void Print(TextWriter writer, int indent)
        {
            writer.Write("(seq "); //no hanging
            writer.Write(Statement, indent + "(seq ".Length);
            writer.WriteLineIndent(indent);
            writer.Write(Tail, indent);
            writer.Write(')');
        }
    }
}
