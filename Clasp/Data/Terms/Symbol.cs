using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;

using Clasp.Data.Static;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.Data.Terms
{
    internal class Symbol : Atom
    {
        public readonly string Name;
        protected Symbol(string name)
        {
            Name = name;
            _interned.Add(name, this);
        }

        private static readonly Dictionary<string, Symbol> _interned = [];
        protected static bool IsInterned(string name) => _interned.ContainsKey(name);

        public static Symbol Intern(string name)
        {
            if (!_interned.ContainsKey(name))
            {
                _interned[name] = new Symbol(name);
            }
            return _interned[name];
        }

        public override string ToString() => Name;
        protected override string FormatType() => "Symbol";
    }

    internal class GenSym : Symbol
    {
        // Gamma, for "GenSym"
        private const string _SEP = "-Γ";

        private static string GenerateUniqueName(string partial)
        {
            string output = partial;
            uint counter = 1;

            while (IsInterned(output))
            {
                output = string.Format("{0}{1}{2}", partial, _SEP, counter++);
            }
            return output;
        }

        public GenSym(string fromName) : base(GenerateUniqueName(fromName)) { }
        public GenSym() : this("GenSym") { }

        protected override string FormatType() => "GenSym";
    }

    /// <summary>
    /// Special symbols that cannot be linguistically represented. They act as
    /// "unshadowable" representations of certain important keywords by dint of being
    /// an entirely different Type.
    /// </summary>
    internal sealed class ReservedSymbol : Symbol
    {
        public ReservedSymbol(string name) : base(name) { }
        protected override string FormatType() => "ReservedSymbol";
    }

    internal static class Symbols
    {
        public static readonly Symbol Quote = Symbol.Intern(Keywords.QUOTE);
        public static readonly Symbol Quasiquote = Symbol.Intern(Keywords.QUASIQUOTE);
        public static readonly Symbol Unquote = Symbol.Intern(Keywords.UNQUOTE);
        public static readonly Symbol UnquoteSplicing = Symbol.Intern(Keywords.UNQUOTE_SPLICING);

        public static readonly Symbol QuoteSyntax = Symbol.Intern(Keywords.QUOTE_SYNTAX);
        public static readonly Symbol Quasisyntax = Symbol.Intern(Keywords.QUASISYNTAX);
        public static readonly Symbol Unsyntax = Symbol.Intern(Keywords.UNSYNTAX);
        public static readonly Symbol UnsyntaxSplicing = Symbol.Intern(Keywords.UNSYNTAX_SPLICING);

        public static readonly Symbol Define = Symbol.Intern(Keywords.DEFINE);
        public static readonly Symbol Set = Symbol.Intern(Keywords.SET);

        public static readonly Symbol If = Symbol.Intern(Keywords.IF);
        public static readonly Symbol Begin = Symbol.Intern(Keywords.BEGIN);
        public static readonly Symbol Apply = Symbol.Intern(Keywords.APPLY);
        public static readonly Symbol Lambda = Symbol.Intern(Keywords.LAMBDA);

        public static readonly Symbol Module = Symbol.Intern(Keywords.MODULE);
        public static readonly Symbol Import = Symbol.Intern(Keywords.IMPORT);
        public static readonly Symbol Export = Symbol.Intern(Keywords.EXPORT);

        public static readonly Symbol DefineSyntax = Symbol.Intern(Keywords.DEFINE_SYNTAX);
        public static readonly Symbol ImportForSyntax = Symbol.Intern(Keywords.IMPORT_FOR_SYNTAX);
        public static readonly Symbol BeginForSyntax = Symbol.Intern(Keywords.BEGIN_FOR_SYNTAX);

        public static readonly Symbol Syntax = Symbol.Intern(Keywords.SYNTAX);
        public static readonly Symbol Ellipsis = Symbol.Intern(Keywords.ELLIPSIS);

        //---

        public static readonly ReservedSymbol S_TopBegin = new(Keywords.S_TOP_BEGIN);
        public static readonly ReservedSymbol S_TopDefine = new(Keywords.S_TOP_DEFINE);
        public static readonly ReservedSymbol S_TopVar = new(Keywords.S_TOP_VAR);

        public static readonly ReservedSymbol S_Module = new(Keywords.S_MODULE);
        public static readonly ReservedSymbol S_ModuleBodyBegin = new(Keywords.S_MODULE_BODY_BEGIN);
        public static readonly ReservedSymbol S_Import = new(Keywords.S_IMPORT);

        public static readonly ReservedSymbol S_Set = new(Keywords.S_SET);

        public static readonly ReservedSymbol S_Meta = new(Keywords.S_META);

        public static readonly ReservedSymbol S_If = new(Keywords.S_IF);
        public static readonly ReservedSymbol S_Begin = new(Keywords.S_BEGIN);
        public static readonly ReservedSymbol S_Apply = new(Keywords.S_APPLY);
        public static readonly ReservedSymbol S_Lambda = new(Keywords.S_LAMBDA);

        public static readonly ReservedSymbol S_Var = new(Keywords.S_VAR);
        public static readonly ReservedSymbol S_Const = new(Keywords.S_CONST);

        //---

        public static readonly ReservedSymbol S_PartialDefine = new(Keywords.S_PARTIAL_DEFINE);
    }
}
