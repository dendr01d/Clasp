using ClaspCompiler.CompilerData;

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

            foreach (object? obj in extra)
            {
                hash.Add(obj);
            }

            return hash.ToHashCode();
        }

        #region Standard

        private static T Predicated<T>(string predName, T type)
            where T : SchemeType
        {
            FunctionType ft = new FunctionType(Boolean, Any)
            {
                LatentPredicate = type
            };
            DefaultBindings.DeclarePredicate(predName, ft);
            return type;
        }

        // TODO there's gotta be some way I can tie this all together a little more neatly
        // mainly with regards to the overriden names and how they're inherited by the corresponding type predicates

        public static readonly AtomicType True = new("True");
        public static readonly AtomicType False = new("False");
        public static readonly SchemeType Boolean = Predicated("boolean?", UnionType.Join(True, False));

        public static readonly AtomicType Integer = Predicated("integer?", new AtomicType("Integer"));
        public static readonly SchemeType Number = Predicated("number?", UnionType.Join(Integer));

        public static readonly AtomicType Symbol = Predicated("symbol?", new AtomicType("Symbol"));

        public static readonly AtomicType Identifier = new("Identifier");
        public static readonly AtomicType SyntaxPair = new("Stx-Pair");
        public static readonly AtomicType SyntaxData = new("Stx-Data");
        public static readonly SchemeType Syntax = Predicated("syntax?", UnionType.Join(Identifier, SyntaxPair, SyntaxData));

        public static readonly AtomicType Top = new("⊤");
        public static readonly SchemeType Bottom = UnionType.Join([], "⊥");

        public static readonly SchemeType Any = Top;
        public static readonly SchemeType Void = Bottom;

        public static readonly AtomicType Nil = Predicated("null?", new AtomicType("Nil"));

        public static SchemeType List(params SchemeType[] types)
        {
            return types.Length == 0
                ? AtomicType.Nil
                : new PairType(types[0], List(types[1..]));
        }
        public static SchemeType ListOf(SchemeType ty) => AllType.Construct(
            x => UnionType.Join(Nil, new PairType(ty, x)),
            (_, _) => $"(Listof {ty})");

        public static void Initialize() { }

        #endregion
    }
}