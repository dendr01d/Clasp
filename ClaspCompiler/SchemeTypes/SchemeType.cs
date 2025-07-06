using ClaspCompiler.CompilerData;

namespace ClaspCompiler.SchemeTypes
{
    internal abstract record SchemeType : IPrintable, IEquatable<SchemeType>, IComparable<SchemeType>
    {
        public TypePredicate? Predicate { get; private set; }

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

        #region Standard

        private static AtomicType InitPredicated(AtomicType ty, string? name = null)
        {
            TypePredicate pred = new(ty, name);
            ty.Predicate = pred;
            return ty;
        }

        private static SchemeType InitPredicated(SchemeType ty, string name)
        {
            TypePredicate pred = new(ty, name);
            ty.Predicate = pred;
            return ty;
        }

        // TODO there's gotta be some way I can tie this all together a little more neatly
        // mainly with regards to the overriden names and how they're inherited by the corresponding type predicates

        public static readonly AtomicType Integer = InitPredicated(new AtomicType("Integer"));
        public static readonly SchemeType Number = InitPredicated(new UnionType(Integer), "number?");

        public static readonly AtomicType True = new("True");
        public static readonly AtomicType False = new("False");
        public static readonly SchemeType Boolean = InitPredicated(new UnionType(True, False), "boolean?");

        public static readonly AtomicType Nil = InitPredicated(new AtomicType("Nil"), "null?");
        public static readonly AtomicType Symbol = InitPredicated(new AtomicType("Symbol"), "null?");

        public static readonly AtomicType Identifier = InitPredicated(new AtomicType("Identifier"));
        public static readonly AtomicType SyntaxPair = InitPredicated(new AtomicType("Stx-Pair"));
        public static readonly AtomicType SyntaxData = InitPredicated(new AtomicType("Stx-Data"));
        public static readonly SchemeType Syntax = InitPredicated(new UnionType(Identifier, SyntaxPair, SyntaxData), "syntax?");

        public static readonly AtomicType Top = new("⊤");
        public static readonly UnionType Bottom = new() { NameOverride = "⊥" };

        public static readonly SchemeType Any = Top;
        public static readonly SchemeType Void = Bottom;

        public static readonly FunctionType PredicateFunction = new(Boolean, Any);

        public static SchemeType List(params SchemeType[] types)
        {
            return types.Length == 0
                ? AtomicType.Nil
                : new PairType(types[0], List(types[1..]));
        }
        public static SchemeType ListOf(SchemeType ty) => new RecursiveType(
            x => new UnionType(Nil, new PairType(ty, x)),
            (_, x) => $"{x}*");

        #endregion
    }
}
