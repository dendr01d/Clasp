
namespace ClaspCompiler.Data
{
    internal abstract record ValueBase<T> : ITerm, IEquatable<ValueBase<T>>
        where T : struct
    {
        public readonly T Value;

        public bool IsAtom => true;
        public bool IsNil => false;

        protected ValueBase(T value) => Value = value;

        public override string ToString() => Value.ToString() ?? "<?>";
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
