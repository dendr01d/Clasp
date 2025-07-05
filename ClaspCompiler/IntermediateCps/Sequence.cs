using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCps.Abstract;

namespace ClaspCompiler.IntermediateCps
{
    internal sealed class Sequence : ITail
    {
        public string ControlCode => "SEQ";

        public IStatement Statement { get; init; }
        public ITail Tail { get; init; }

        public Dictionary<VarBase, int> FreeVariables { get; }

        public Sequence(IStatement stmt, ITail tail)
        {
            Statement = stmt;
            Tail = tail;

            FreeVariables = new();
            foreach (var pair in stmt.FreeVariables.Concat(tail.FreeVariables))
            {
                if (!FreeVariables.ContainsKey(pair.Key))
                {
                    FreeVariables[pair.Key] = 0;
                }
                FreeVariables[pair.Key] += pair.Value;
            }
        }

        public bool BreaksLine => true;
        public string AsString => $"({ControlCode} {Statement} {Tail})";
        public void Print(TextWriter writer, int indent)
        {
            string lead = $"({ControlCode} ";

            writer.Write(lead); //no indentation
            writer.Write(Statement, indent + lead.Length);
            writer.WriteLineIndent(indent);
            writer.Write(Tail, indent);
            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}
