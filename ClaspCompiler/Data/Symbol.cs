namespace ClaspCompiler.Data
{
    internal sealed record Symbol : ITerm, IEquatable<Symbol>
    {
        public readonly string Name;

        public bool IsAtom => true;
        public bool IsNil => false;

        public Symbol(string name) => Name = name;

        private static uint _counter = 0;
        public static Symbol GenSym()
        {
            return new Symbol($"$.{++_counter}");
        }

        public override string ToString() => Name;
        public void Print(TextWriter writer, int indent) => writer.Write(Name);
    }
}
