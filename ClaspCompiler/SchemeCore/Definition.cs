using ClaspCompiler.SchemeCore.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeCore
{
    internal sealed class Definition : ICoreDef
    {
        public ICoreVar Variable { get; init; }
        public ICoreExp Value { get; init; }
        public static SchemeType Type => AtomicType.Void;

        public Definition(ICoreVar var, ICoreExp newValue)
        {
            Variable = var;
            Value = newValue;
        }

        public bool BreaksLine => true;
        public string AsString => $"(define {Variable} {Value})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication("define", [Variable, Value], indent);
        public sealed override string ToString() => AsString;
    }
}
