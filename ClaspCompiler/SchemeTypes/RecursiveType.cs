namespace ClaspCompiler.SchemeTypes
{
    internal sealed record RecursiveType : SchemeType
    {
        public VarType Representative { get; init; }
        public SchemeType RecurrentType { get; init; }
        private Func<VarType, SchemeType, string>? _formatter { get; init; }

        public RecursiveType(Func<VarType, SchemeType> constructor, Func<VarType, SchemeType, string>? formatter = null)
        {
            Representative = new VarType();
            RecurrentType = constructor.Invoke(Representative);
            _formatter = formatter;
        }

        public override string AsString => _formatter?.Invoke(Representative, RecurrentType) ?? $"(Rec {Representative} {RecurrentType})";
    }
}
