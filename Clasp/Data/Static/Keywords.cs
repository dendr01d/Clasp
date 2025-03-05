using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Static
{
    /// <summary>
    /// The default keywords used for special forms (or implicit macros) within the language.
    /// </summary>
    internal static class Keywords
    {
        //--------------
        // Misc Keyword
        //--------------
        public const string IMPLICIT_MODULE = "#clasp";

        //------------------------
        // Surface-Level Keywords
        //------------------------
        public const string QUOTE = "quote";
        public const string QUASIQUOTE = "quasiquote";
        public const string UNQUOTE = "unquote";
        public const string UNQUOTE_SPLICING = "unquote-splicing";

        public const string QUOTE_SYNTAX = "quote-syntax";
        public const string QUASISYNTAX = "quasisyntax";
        public const string UNSYNTAX = "unsyntax";
        public const string UNSYNTAX_SPLICING = "unsyntax-splicing";

        public const string DEFINE = "define";
        public const string SET = "set!";

        public const string IF = "if";
        public const string BEGIN = "begin";
        public const string APPLY = "apply";
        public const string LAMBDA = "lambda";

        public const string MODULE = "module";
        public const string IMPORT = "import";
        public const string EXPORT = "export";

        public const string DEFINE_SYNTAX = "define-syntax";
        public const string BEGIN_FOR_SYNTAX = "begin-for-syntax";
        public const string IMPORT_FOR_SYNTAX = "import-for-syntax";

        public const string SYNTAX = "syntax";
        public const string ELLIPSIS = "...";

        //-----------------
        // Secret Keywords
        //-----------------
        public const string S_TOP_VAR = "σ-top-var";
        public const string S_TOP_BEGIN = "σ-top-begin";
        public const string S_TOP_DEFINE = "σ-top-define";
        public const string S_MODULE = "σ-module";
        public const string S_MODULE_BEGIN = "σ-module-begin";
        public const string S_IMPORT = "σ-import";
        public const string S_SET = "σ-set!";
        public const string S_IF = "σ-if";
        public const string S_BEGIN = "σ-begin";
        public const string S_APPLY = "σ-apply";
        public const string S_LAMBDA = "σ-lambda";
        public const string S_VAR = "σ-var";
        public const string S_CONST = "σ-const";
        public const string S_CONST_SYNTAX = "σ-const-syntax";

        public static readonly string[] SecretKeywords = [S_TOP_VAR, S_TOP_BEGIN, S_TOP_DEFINE, S_MODULE, S_IMPORT, S_SET, S_IF, S_BEGIN, S_APPLY, S_LAMBDA, S_VAR, S_CONST];

        // These don't represent core forms, just hints for the expander/parser
        public const string S_META = "σ-meta";

        public const string S_PARTIAL_DEFINE = "σ-partial-define";
    }
}
