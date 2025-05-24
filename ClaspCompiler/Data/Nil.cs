namespace ClaspCompiler.Data
{
    internal sealed class Nil : ITerm, IEquatable<Nil>
    {
        public static readonly Nil Instance = new();

        public bool IsAtom => true;
        public bool IsNil => true;

        private Nil() { }

        public override bool Equals(object? obj) => obj is Nil;
        public bool Equals(ITerm? other) => other is Nil;
        public bool Equals(Nil? other) => other is not null;
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString() => "()";
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());

    }
}
