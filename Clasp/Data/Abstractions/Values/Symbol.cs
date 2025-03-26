namespace Clasp.Data.Abstractions.Values
{
    internal readonly struct Symbol
    {
        public readonly string Name;

        public Symbol(string name)
        {
            Name = name;
        }
    }
}
