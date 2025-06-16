namespace ClaspCompiler.SchemeTypes
{
    /// <summary>
    /// Represents a type that is unidentified as of yet
    /// </summary>
    internal sealed record TypeVar : SchemeType
    {
        private static uint _counter = 1;

        public readonly uint Id;
        public TypeVar() => Id = _counter++;
        public override string AsString => $"T<{Id}>";
    }
}
