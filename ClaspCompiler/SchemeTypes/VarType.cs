namespace ClaspCompiler.SchemeTypes
{
    // A variable representing an unknown (but consistent) type value
    internal sealed record VarType() : SchemeType
    {
        private static uint _idCounter = 1;

        public uint Id { get; } = _idCounter++;

        public override string AsString => $"T{Id}";
    }
}
