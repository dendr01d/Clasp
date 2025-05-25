using ClaspCompiler.Data;
using ClaspCompiler.PseudoIl;
using ClaspCompiler.Semantics;

namespace ClaspCompiler.Common
{
    internal sealed record Var : ILiteral, IMem
    {
        public Symbol Symbol { get; init; }

        public Var(Symbol value) => Symbol = value;
        public ITerm GetValue() => Symbol;
        public string GetTypeName() => "var";

        public static Var GenVar(Var? var = null) => new Var(Symbol.GenSym(var?.Symbol.Name));

        public override string ToString() => string.Format("({0} {1})", "var", Symbol);
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
