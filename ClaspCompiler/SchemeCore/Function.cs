using ClaspCompiler.SchemeCore.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeCore
{
    internal sealed class Function : ICoreExp
    {
        private const char LAMBDA = '\u03BB';

        public readonly ICoreVar Variable;
        public readonly ICoreExp Body;
        public SchemeType Type { get; init; }

        public Function(ICoreVar var, ICoreExp body, SchemeType type)
        {
            Variable = var;
            Body = body;
            Type = type;
        }

        public bool BreaksLine => true;
        public string AsString => $"({LAMBDA} ({Variable}) {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting($"({LAMBDA} ", ref indent);
            writer.Write('(');
            writer.Write(Variable, indent);
            writer.Write(')');

            writer.WriteLineIndent(indent);
            writer.Write(Body);
            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}
