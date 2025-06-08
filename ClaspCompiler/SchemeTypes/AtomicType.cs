namespace ClaspCompiler.SchemeTypes
{
    internal sealed record AtomicType : SchemeType
    {
        private readonly string _typeName;
        private AtomicType(string typeName) => _typeName = typeName;
        public override string AsString => _typeName;

        public static readonly AtomicType Integer = new("Integer");
        public static readonly AtomicType Boole = new("Boolean");

        public static readonly AtomicType Void = new("Void");
    }
}
