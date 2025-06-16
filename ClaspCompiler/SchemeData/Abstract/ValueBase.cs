using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeData.Abstract
{
    /// <summary>
    /// A base for specifying shared behavior of certain <see cref="IValue"/> types.
    /// </summary>
    internal abstract record ValueBase<T> : IEquatable<ValueBase<T>>, IValue
        where T : struct
    {
        public readonly T Value;

        public SchemeType Type { get; init; }
        public bool IsAtom => true;
        public bool IsNil => false;

        protected ValueBase(T value, SchemeType type)
        {
            Value = value;
            Type = type;
        }

        public bool BreaksLine => false;
        public virtual string AsString => Value.ToString() ?? "<?>";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;

        public bool Equals(ICpsExp? other) => other is ValueBase<T> vb && Value.Equals(vb.Value);
    }
}
