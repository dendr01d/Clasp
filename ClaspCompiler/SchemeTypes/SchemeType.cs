namespace ClaspCompiler.SchemeTypes
{
    internal abstract record SchemeType : IPrintable, IEquatable<SchemeType>, IComparable<SchemeType>
    {
        public virtual bool BreaksLine => false;
        public abstract string AsString { get; }
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;

        public int CompareTo(SchemeType? other) => AsString.CompareTo(other?.AsString ?? string.Empty);

        protected static int CreateVariadicHash(string typeName, IEnumerable<SchemeType> types, params object?[] extra)
        {
            HashCode hash = new();
            hash.Add(typeName);

            foreach (SchemeType type in types)
            {
                hash.Add(type);
            }

            foreach(object? obj in extra)
            {
                hash.Add(obj);
            }

            return hash.ToHashCode();
        }
    }
}
