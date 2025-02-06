using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data
{
    /// <summary>
    /// The default keywords used for special forms within the language.
    /// </summary>
    internal static class Keyword
    {
        public const string DEFINE = "define";
        public const string DEFINE_SYNTAX = "define-syntax";
        public const string SET = "set!";

        public const string BEGIN = "begin";
        public const string IF = "if";
        public const string LAMBDA = "lambda";

        public const string QUOTE = "quote";
        public const string QUASIQUOTE = "quasiquote";
        public const string UNQUOTE = "unquote";
        public const string UNQUOTE_SPLICING = "unquote-splicing";

        public const string QUOTE_SYNTAX = "quote-syntax";
        public const string LET_SYNTAX = "let-syntax";

        public const string SYNTAX = "syntax";
        public const string QUASISYNTAX = "quasisyntax";
        public const string UNSYNTAX = "unsyntax";
        public const string UNSYNTAX_SPLICING = "unsyntax-splicing";

        public const string ELLIPSIS = "...";

        public const string IMP_APP = "#%app";
        public const string IMP_DATUM = "#%datum";
        public const string IMP_TOP = "#%top";
        public const string IMP_LAMBDA = "#%lambda";
        public const string IMP_SEQ = "#%seq";
        public const string IMP_VAR = "#%var";
        public const string IMP_PARDEF = "#%partial-define";

    }
}
