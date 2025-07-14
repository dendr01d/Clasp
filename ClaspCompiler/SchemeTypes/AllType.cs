namespace ClaspCompiler.SchemeTypes
{
    internal sealed record AllType(VarType[] VarTypes, VarType? DottedVarType, SchemeType ScopedType) : SchemeType
    {
        private Func<VarType[], SchemeType, string>? _formatter { get; init; } = null;

        private static readonly string[] _placeholders = ["α", "β", "γ", "δ", "ε", "ζ", "η", "θ", "ι", "κ", "λ", "μ", "ν", "ξ", "ο", "π", "ρ", "ς", "σ", "τ", "υ", "φ", "χ", "ψ", "ω"];

        public override string AsString => _formatter?.Invoke(VarTypes, ScopedType)
            ?? $"(∀ ({string.Join(' ', VarTypes.AsEnumerable())}) {ScopedType})";

        public static AllType Construct(Func<VarType, SchemeType> constructor, Func<VarType[], SchemeType, string>? formatter = null)
            => Construct(1, x => constructor(x[0]), formatter);

        public static AllType Construct(int arity, Func<VarType[], SchemeType> constructor, Func<VarType[], SchemeType, string>? formatter = null)
        {
            VarType[] vars = [.. Enumerable.Range(0, arity).Select(x => new VarType())];
            SchemeType scoped = constructor(vars);
            return new AllType(vars, null, scoped)
            {
                _formatter = formatter
            };
        }

        public static AllType ConstructDotted(Func<VarType, SchemeType> constructor, Func<VarType[], SchemeType, string>? formatter = null)
            => ConstructDotted(0, (x, y) => constructor(y), formatter);

        public static AllType ConstructDotted(int arity, Func<VarType[], VarType, SchemeType> constructor, Func<VarType[], SchemeType, string>? formatter = null)
        {
            VarType[] vars = [.. Enumerable.Range(0, arity).Select(x => new VarType())];
            VarType dottedVar = new();
            SchemeType scoped = constructor(vars, dottedVar);
            return new AllType(vars, null, scoped)
            {
                _formatter = formatter
            };
        }
    }
}
