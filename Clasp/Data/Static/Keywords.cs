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
        public const string STATIC_TOP = "σtop";
        public const string STATIC_VAR = "σvar";

        public const string QUOTE = "quote";
        public const string STATIC_QUOTE = "σquote";
        public const string QUASIQUOTE = "quasiquote";
        public const string UNQUOTE = "unquote";
        public const string UNQUOTE_SPLICING = "unquote-splicing";

        public const string QUOTE_SYNTAX = "quote-syntax";
        public const string QUASISYNTAX = "quasisyntax";
        public const string UNSYNTAX = "unsyntax";
        public const string UNSYNTAX_SPLICING = "unsyntax-splicing";

        public const string APPLY = "apply";
        public const string STATIC_APPLY = "σapp";

        public const string DEFINE = "define";
        public const string STATIC_PARDEF = "σpart-define";
        public const string SET = "set!";

        public const string LAMBDA = "lambda";
        public const string STATIC_LAMBDA = "σlambda";

        public const string IF = "if";

        public const string BEGIN = "begin";
        public const string STATIC_BEGIN = "σbegin";

        public const string MODULE = "module";
        public const string IMPLICIT_MODULE = "#clasp";
        public const string STATIC_PARMOD = "σpart-module";
        public const string IMPORT = "import";
        public const string EXPORT = "export";

        public const string STATIC_MODULE_BEGIN = "σmodule-body-begin";

        public const string DEFINE_SYNTAX = "define-syntax";
        public const string BEGIN_FOR_SYNTAX = "begin-for-syntax";
        public const string IMPORT_FOR_SYNTAX = "import-for-syntax";

        public const string SYNTAX = "syntax";
        public const string ELLIPSIS = "...";
    }
}
