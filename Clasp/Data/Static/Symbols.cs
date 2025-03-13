using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;

namespace Clasp.Data.Static
{
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
        //public static readonly Symbol ImportFrom = Symbol.Intern(Keywords.IMPORT_FROM);
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
        public static readonly ReservedSymbol S_Module_Begin = new(Keywords.S_MODULE_BEGIN);
        public static readonly ReservedSymbol S_Import = new(Keywords.S_IMPORT);
        //public static readonly ReservedSymbol S_ImportFrom = new(Keywords.S_IMPORT_FROM);

        public static readonly ReservedSymbol S_Set = new(Keywords.S_SET);

        public static readonly ReservedSymbol S_If = new(Keywords.S_IF);
        public static readonly ReservedSymbol S_Begin = new(Keywords.S_BEGIN);
        public static readonly ReservedSymbol S_App = new(Keywords.S_APP);
        public static readonly ReservedSymbol S_Lambda = new(Keywords.S_LAMBDA);

        public static readonly ReservedSymbol S_Var = new(Keywords.S_VAR);
        public static readonly ReservedSymbol S_Const = new(Keywords.S_CONST);
        public static readonly ReservedSymbol S_Const_Syntax = new(Keywords.S_CONST_SYNTAX);

        //---

        public static readonly ReservedSymbol S_PartialDefine = new(Keywords.S_PARTIAL_DEFINE);
        public static readonly ReservedSymbol S_VisitModule = new(Keywords.S_VISIT_MODULE);
    }
}
