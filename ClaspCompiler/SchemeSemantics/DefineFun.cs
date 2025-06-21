using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record DefineFun(ISemVar Variable, ParamsForm Parameters, ISemExp Value) : ISemDef
    {
        public bool BreaksLine => Value.BreaksLine;
        public string AsString => string.Concat([
            $"(define ({Variable}",
            Parameters.Any ? $" {Parameters.AsStandalone}" : string.Empty,
            ") ",
            Value,
            ")"
        ]);
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(define ", ref indent);
            writer.Write('(');
            writer.Write(Variable);
            if (Parameters.Any)
            {
                writer.Write(' ');
                Parameters.Print(writer, indent);
            }
            writer.Write(')');

            writer.WriteLineIndent(indent);
            writer.Write(Value, indent);
            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}
