namespace ClaspCompiler.SchemeTypes
{
    internal sealed record VarType : SchemeType
    {
        private static uint _counter = 0;

        public readonly uint Id;

        public VarType() => Id = ++_counter;

        public override string AsString => $"T<{Id}>";
    }
}
