using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.CompilerData
{
    /// <summary>
    /// Represents a primitive operator for a function executed by the .NET runtime.
    /// </summary>
    internal sealed class PrimitiveOperator : IPrintable
    {
        public string Name { get; init; }
        public Symbol Symbol { get; init; }
        public Identifier Identifier { get; init; }

        public FunctionType Type { get; init; }
        public bool SideEffective { get; init; }

        private PrimitiveOperator(string name, Symbol sym, FunctionType type, bool sideEffect)
        {
            Name = name;
            Symbol = sym;
            Identifier = new(sym, [], SourceRef.DefaultSyntax);

            Type = type;
            SideEffective = sideEffect;
        }

        public static PrimitiveOperator Init(string name, FunctionType type, bool sideEffect = false)
        {
            Symbol sym = SymbolFactory.InternGlobal(name);

            PrimitiveOperator op = new(name, sym, type, sideEffect);

            DefaultBindings.AddPrimitive(op);

            return op;
        }

        public bool BreaksLine => false;
        public string AsString => $"<{Name}>";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;

        #region Standard Operators

        public static readonly PrimitiveOperator IsNull = Init("null?", new FunctionType(UnionType.Boole, [AtomicType.Any]));

        public static readonly PrimitiveOperator Read = Init("read", new FunctionType(AtomicType.Integer, []), true);

        public static readonly PrimitiveOperator Add = Init("+", new FunctionType(AtomicType.Integer, [new ListOfType(AtomicType.Integer)]));
        public static readonly PrimitiveOperator Sub = Init("-", new FunctionType(AtomicType.Integer, [AtomicType.Integer, AtomicType.Integer]));
        public static readonly PrimitiveOperator Neg = Init("-", new FunctionType(AtomicType.Integer, [AtomicType.Integer]));

        public static readonly PrimitiveOperator Eq = Init("eq?", new FunctionType(UnionType.Boole, [AtomicType.Any, AtomicType.Any]));
        public static readonly PrimitiveOperator Eqv = Init("eqv?", new FunctionType(UnionType.Boole, [AtomicType.Any, AtomicType.Any]));

        public static readonly PrimitiveOperator Lt = Init("<", new FunctionType(UnionType.Boole, [AtomicType.Integer, AtomicType.Integer]));
        public static readonly PrimitiveOperator LtE = Init("<=", new FunctionType(UnionType.Boole, [AtomicType.Integer, AtomicType.Integer]));
        public static readonly PrimitiveOperator Gt = Init(">", new FunctionType(UnionType.Boole, [AtomicType.Integer, AtomicType.Integer]));
        public static readonly PrimitiveOperator GtE = Init(">=", new FunctionType(UnionType.Boole, [AtomicType.Integer, AtomicType.Integer]));

        public static readonly PrimitiveOperator Not = Init("not", new FunctionType(UnionType.Boole, [AtomicType.Any]));

        public static readonly PrimitiveOperator SyntaxExpose = Init("stx-e", new FunctionType(AtomicType.Any, [UnionType.Syntax]));
        public static readonly PrimitiveOperator MakeSyntax = Init("mk-stx", new FunctionType(UnionType.Syntax, [AtomicType.Any, UnionType.Syntax]));

        public static readonly PrimitiveOperator Void = Init("void", new FunctionType(AtomicType.Void, []));

        public static readonly PrimitiveOperator Cons;
        public static readonly PrimitiveOperator Car;
        public static readonly PrimitiveOperator Cdr;

        static PrimitiveOperator()
        {
            VarType consV1 = new();
            VarType consV2 = new();
            Cons = Init("cons", new FunctionType(new PairType(consV1, consV2), [consV1, consV2]));

            VarType carV = new();
            Car = Init("car", new FunctionType(carV, [new PairType(carV, AtomicType.Any)]));

            VarType cdrV = new();
            Cdr = Init("cdr", new FunctionType(cdrV, [new PairType(AtomicType.Any, cdrV)]));
        }

        /// <summary>
        /// Forces the static constructor to run.
        /// </summary>
        public static void Initialize() { }

        #endregion
    }
}