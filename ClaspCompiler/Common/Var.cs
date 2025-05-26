using ClaspCompiler.Data;
using ClaspCompiler.PseudoIl;

namespace ClaspCompiler.Common
{
    internal sealed record Var : IAtom, IMem
    {
        public TypeName TypeName => TypeName.Symbol;
        public Symbol Data { get; init; }

        public Var(Symbol value) => Data = value;
        public ITerm GetValue() => Data;
        public string GetTypeName() => "var";

        public static Var GenVar(Var? var = null) => new(Symbol.GenSym(var?.Data.Name));

        public override string ToString() => string.Format("({0} {1})", "var", Data);
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
