using ClaspCompiler.IntermediateAnfLang.Abstract;
using ClaspCompiler.IntermediateVarLang.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.CompilerData
{
    internal sealed record Var : IPrintable,
        ISemanticExp,
        INormArg,
        ILocArg
    {
        public string Name { get; init; }
        public Var(string name) => Name = name;


        private static uint _counter = 0;
        public static void ResetGenerator() => _counter = 0;
        private const string DEFAULT_NAME = "$";
        public static Var Gen(string? name = null)
        {
            return new Var($"{name ?? DEFAULT_NAME}.{++_counter}");
        }


        public bool CanBreak => false;
        public override string ToString() => string.Format("({0} {1})", "var", Name);
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
