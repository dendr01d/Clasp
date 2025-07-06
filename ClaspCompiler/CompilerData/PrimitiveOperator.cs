using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.CompilerData
{
    /// <summary>
    /// Represents a primitive operator for a function executed by the .NET runtime.
    /// </summary>
    internal class PrimitiveOperator : IPrintable
    {
        public string Name { get; init; }
        public Symbol Symbol { get; init; }
        public Identifier Identifier { get; init; }

        public FunctionType Type { get; init; }
        public bool SideEffective { get; init; }

        protected PrimitiveOperator(string name, FunctionType type, bool hasSideEffect)
        {
            Name = name;
            Symbol = SymbolFactory.InternGlobal(name);
            Identifier = new(Symbol, [], SourceRef.DefaultSyntax);

            Type = type;
            SideEffective = hasSideEffect;
        }

        private static PrimitiveOperator Init(string name, FunctionType type, bool hasSideEffect = false)
        {
            PrimitiveOperator op = new(name, type, hasSideEffect);

            DefaultBindings.AddPrimitive(op);

            return op;
        }

        private static PrimitiveOperator Init(string name, Func<VarType, FunctionType> constructor, bool hasSideEffect = false)
        {
            return Init(name, constructor(new VarType()), hasSideEffect);
        }

        private static PrimitiveOperator Init(string name, Func<VarType, VarType, FunctionType> constructor, bool hasSideEffect = false)
        {
            return Init(name, constructor(new VarType(), new VarType()), hasSideEffect);
        }

        public bool BreaksLine => false;
        public string AsString => $"<{Name}>";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;

        #region Standard Operators

        public static readonly PrimitiveOperator Read = Init("read", new FunctionType(SchemeType.Integer, []), true);

        public static readonly PrimitiveOperator Add = Init("+", new FunctionType(SchemeType.Integer, SchemeType.ListOf(SchemeType.Integer)));
        public static readonly PrimitiveOperator Sub = Init("-", new FunctionType(SchemeType.Integer, [SchemeType.Integer, SchemeType.Integer]));
        public static readonly PrimitiveOperator Neg = Init("-", new FunctionType(SchemeType.Integer, [SchemeType.Integer]));

        public static readonly PrimitiveOperator Eq = Init("eq?", new FunctionType(SchemeType.Boolean, [SchemeType.Any, SchemeType.Any]));
        public static readonly PrimitiveOperator Eqv = Init("eqv?", new FunctionType(SchemeType.Boolean, [SchemeType.Any, SchemeType.Any]));

        public static readonly PrimitiveOperator Lt = Init("<", new FunctionType(SchemeType.Boolean, [SchemeType.Integer, SchemeType.Integer]));
        public static readonly PrimitiveOperator LtE = Init("<=", new FunctionType(SchemeType.Boolean, [SchemeType.Integer, SchemeType.Integer]));
        public static readonly PrimitiveOperator Gt = Init(">", new FunctionType(SchemeType.Boolean, [SchemeType.Integer, SchemeType.Integer]));
        public static readonly PrimitiveOperator GtE = Init(">=", new FunctionType(SchemeType.Boolean, [SchemeType.Integer, SchemeType.Integer]));

        public static readonly PrimitiveOperator Not = Init("not", new FunctionType(SchemeType.Boolean, SchemeType.Any));

        public static readonly PrimitiveOperator SyntaxExpose = Init("stx-e", new FunctionType(SchemeType.Any, [SchemeType.Syntax]));
        public static readonly PrimitiveOperator MakeSyntax = Init("mk-stx", new FunctionType(SchemeType.Syntax, [SchemeType.Any, SchemeType.Syntax]));

        public static readonly PrimitiveOperator Void = Init("void", new FunctionType(SchemeType.Void, []));

        public static readonly PrimitiveOperator Cons = Init("cons", (x, y) => new FunctionType(new PairType(x, y), [x, y]));
        public static readonly PrimitiveOperator Car = Init("car", x => new FunctionType(x, [new PairType(x, SchemeType.Any)]));
        public static readonly PrimitiveOperator Cdr = Init("cdr", x => new FunctionType(x, [new PairType(SchemeType.Any, x)]));

        static PrimitiveOperator()
        { }

        /// <summary>
        /// Forces the static constructor to run.
        /// </summary>
        public static void Initialize() { }

        #endregion
    }
}