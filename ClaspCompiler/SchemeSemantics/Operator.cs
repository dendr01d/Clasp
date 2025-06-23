using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    /// <summary>
    /// A primitive data operation
    /// </summary>
    internal class Operator : ISemExp
    {
        public readonly string Name;
        public MetaData MetaData { get; init; }

        private Operator(string name, FunctionType type)
        {
            Name = name;
            MetaData = new MetaData()
            {
                Type = type
            };
        }

        public bool BreaksLine => false;
        public string AsString => Name;
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;

        // ---

        public static readonly Operator Add = new(Keyword.PLUS, new(new ProductType(AtomicType.Integer, AtomicType.Integer), AtomicType.Integer));
        public static readonly Operator Sub = new(Keyword.MINUS, new(new ProductType(AtomicType.Integer, AtomicType.Integer), AtomicType.Integer));

        public static readonly Operator Neg = new(Keyword.MINUS, new(AtomicType.Integer, AtomicType.Integer));

        public static readonly Operator Read = new(Keyword.READ, new(AtomicType.Void, AtomicType.Integer));

        public static readonly Operator Not = new(Keyword.NOT, new(AtomicType.Any, AtomicType.Boole));

        public static readonly Operator Eq = new(Keyword.EQ, new(new ProductType(AtomicType.Any, AtomicType.Any), AtomicType.Boole));
        public static readonly Operator Lt = new(Keyword.LT, new(new ProductType(AtomicType.Integer, AtomicType.Integer), AtomicType.Boole));
        public static readonly Operator LtEq = new(Keyword.LTE, new(new ProductType(AtomicType.Integer, AtomicType.Integer), AtomicType.Boole));
        public static readonly Operator Gt = new(Keyword.GT, new(new ProductType(AtomicType.Integer, AtomicType.Integer), AtomicType.Boole));
        public static readonly Operator GtEq = new(Keyword.GTE, new(new ProductType(AtomicType.Integer, AtomicType.Integer), AtomicType.Boole));
    }
}
