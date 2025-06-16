using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    /// <summary>
    /// Represents the evaluation of a form after binding an argument to a variable
    /// </summary>
    internal sealed class Lambda : ISemExp
    {
        public SemVar Variable { get; init; }
        public ISemExp Body { get; init; }
        public MetaData MetaData { get; init; }

        public Lambda(SemVar var, ISemExp body, MetaData? meta = null)
        {
            Variable = var;
            Body = body;
            MetaData = meta ?? new();
        }

        public bool BreaksLine => true;
        public string AsString => string.Format("(lambda ({0}) {1})", Variable, Body);
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(lambda ", ref indent);
            int restIndent = indent;

            writer.Write('(');
            writer.Write(Variable, indent);
            writer.WriteLineIndent(")", indent);

            writer.Write(Body, restIndent);

            writer.Write(')');
        }
        public override string ToString() => AsString;
    }
}
