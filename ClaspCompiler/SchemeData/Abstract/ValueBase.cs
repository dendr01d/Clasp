namespace ClaspCompiler.SchemeData.Abstract
{
    internal abstract record ValueBase<T> : IEquatable<ValueBase<T>>, IPrintable
        where T : struct
    {
        public readonly T Value;

        public bool IsAtom => true;
        public bool IsNil => false;

        protected ValueBase(T value) => Value = value;

        public bool CanBreak => false;
        public sealed override string ToString() => Value.ToString() ?? "<?>";
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
