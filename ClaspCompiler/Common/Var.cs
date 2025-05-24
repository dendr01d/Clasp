using ClaspCompiler.Data;
using ClaspCompiler.Semantics;

namespace ClaspCompiler.Common
{
    internal sealed record Var : ILiteral, ISemExp
    {
        public Symbol Symbol { get; init; }

        public Var(Symbol value) => Symbol = value;
        public ITerm GetValue() => Symbol;

        public static Var GenVar(Var? var = null) => new Var(Symbol.GenSym(var?.Symbol.Name));

        public override string ToString() => Symbol.ToString();
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
