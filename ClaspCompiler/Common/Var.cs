using ClaspCompiler.Data;
using ClaspCompiler.Semantics;

namespace ClaspCompiler.Common
{
    internal sealed record Var : ILiteral, ISemExp
    {
        public Symbol Symbol { get; init; }
        ITerm ILiteral.Value => Symbol;

        public Var(Symbol value) => Symbol = value;

        public static Var GenVar() => new Var(Symbol.GenSym());

        public override string ToString() => Symbol.ToString();
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
