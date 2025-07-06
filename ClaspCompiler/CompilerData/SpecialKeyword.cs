using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.Text;

namespace ClaspCompiler.CompilerData
{
    internal class SpecialKeyword : IPrintable
    {
        public string Name { get; init; }
        public Symbol Symbol { get; init; }
        public Identifier Identifier { get; init; }

        private SpecialKeyword(string name, Symbol sym)
        {
            Name = name;
            Symbol = sym;
            Identifier = new(sym, [], SourceRef.DefaultSyntax);
        }

        private static HashSet<string> _names = [];

        private static SpecialKeyword Init(string name)
        {
            _names.Add(name);

            Symbol sym = SymbolFactory.InternGlobal(name);

            SpecialKeyword output = new(name, sym);

            DefaultBindings.AddSpecial(output);

            return output;
        }

        public static bool IsKeyword(string name) => _names.Contains(name);

        #region Standard

        public static readonly SpecialKeyword Apply = Init("apply");
        public static readonly SpecialKeyword Begin = Init("begin");
        public static readonly SpecialKeyword BeginMeta = Init("begin-meta");
        public static readonly SpecialKeyword Define = Init("define");
        public static readonly SpecialKeyword If = Init("if");
        public static readonly SpecialKeyword Lambda = Init("lambda");
        public static readonly SpecialKeyword SetBang = Init("set!");
        public static readonly SpecialKeyword Quote = Init("quote");
        public static readonly SpecialKeyword Values = Init("values");

        public static readonly SpecialKeyword Quasiquote = Init("quasiquote");
        public static readonly SpecialKeyword Unquote = Init("unquote");
        public static readonly SpecialKeyword UnquoteSplicing = Init("unquote-splicing");

        //public static readonly SpecialKeyword DefineSyntax = Init("define-syntax");
        //public static readonly SpecialKeyword QuoteSyntax = Init("quote-syntax");

        static SpecialKeyword() { }

        /// <summary>
        /// Forces the static constructor to run.
        /// </summary>
        public static void Initialize() { }

        #endregion

        public int StructuralDepth => 0;
        public bool BreaksLine => false;
        public string AsString => Name.ToUpper();
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}