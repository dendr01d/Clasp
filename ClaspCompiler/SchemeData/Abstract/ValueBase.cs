using ClaspCompiler.IntermediateCps.Abstract;

namespace ClaspCompiler.SchemeData.Abstract
{
    /// <summary>
    /// A base for specifying shared behavior of certain <see cref="IValue"/> types.
    /// </summary>
    internal abstract record ValueBase<T> : IEquatable<ValueBase<T>>, IValue
        where T : struct
    {
        public readonly T Value;

        public bool IsAtom => true;
        public bool IsNil => false;

        protected ValueBase(T value) => Value = value;

        public bool BreaksLine => false;
        public string AsString => Value.ToString() ?? "<?>";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;

        public bool Equals(ICpsExp? other) => other is ValueBase<T> vb && Value.Equals(vb.Value);
    }
}
