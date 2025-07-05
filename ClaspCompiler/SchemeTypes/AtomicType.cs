namespace ClaspCompiler.SchemeTypes
{
    internal sealed record AtomicType : SchemeType
    {
        private readonly string _typeName;
        private AtomicType(string typeName) => _typeName = typeName;
        public override string AsString => _typeName;


        public static readonly AtomicType Integer = new("Integer");
        public static readonly AtomicType Boole = new("Boolean");

        public static readonly AtomicType Nil = new("Nil");
        public static readonly AtomicType Symbol = new("Symbol");

        public static readonly AtomicType Identifier = new("Identifier");
        public static readonly AtomicType SyntaxPair = new("Stx-Pair");
        public static readonly AtomicType SyntaxData = new("Stx-Datum");

        public static readonly AtomicType Any = new("Any");

        public static readonly AtomicType Void = new("Void");
        public static readonly AtomicType Undefined = new("Undefined");
    }
}
