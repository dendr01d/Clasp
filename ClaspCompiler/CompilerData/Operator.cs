using ClaspCompiler.SchemeCore.Abstract;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.CompilerData
{
    /// <summary>
    /// Represents a primitive operator for a function executed by the .NET runtime.
    /// </summary>
    internal sealed class Operator : IPrintable, ICoreOperator
    {
        public string Name { get; init; }
        public Symbol Symbol { get; init; }
        public FunctionType Type { get; init; }
        public bool SideEffective { get; init; }

        SchemeType ICoreExp.Type => Type;

        private Operator(string name, FunctionType type, bool sideEffect = false)
        {
            Name = name;
            Symbol = SymbolFactory.InternGlobal(name);
            Type = type;
            SideEffective = sideEffect;

            DefaultBindings.AddDefault(Symbol);
        }

        public bool BreaksLine => false;
        public string AsString => $"<{Name}>";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;

        #region Standard Operators

        public static readonly Operator Read = new("read", new FunctionType(AtomicType.Integer), true);

        public static readonly Operator Add = new("+", new FunctionType(AtomicType.Integer, AtomicType.Integer, AtomicType.Integer));
        public static readonly Operator Sub = new("-", new FunctionType(AtomicType.Integer, AtomicType.Integer, AtomicType.Integer));
        public static readonly Operator Neg = new("-", new FunctionType(AtomicType.Integer, AtomicType.Integer));

        public static readonly Operator Eq = new("eq?", new FunctionType(AtomicType.Boole, AtomicType.Any, AtomicType.Any));
        public static readonly Operator Eqv = new("eqv?", new FunctionType(AtomicType.Boole, AtomicType.Any, AtomicType.Any));

        public static readonly Operator Lt = new("<", new FunctionType(AtomicType.Boole, AtomicType.Integer, AtomicType.Integer));
        public static readonly Operator LtE = new("<=", new FunctionType(AtomicType.Boole, AtomicType.Integer, AtomicType.Integer));
        public static readonly Operator Gt = new(">", new FunctionType(AtomicType.Boole, AtomicType.Integer, AtomicType.Integer));
        public static readonly Operator GtE = new(">=", new FunctionType(AtomicType.Boole, AtomicType.Integer, AtomicType.Integer));

        public static readonly Operator Not = new("not", new FunctionType(AtomicType.Boole, AtomicType.Any));

        #endregion
    }
}
