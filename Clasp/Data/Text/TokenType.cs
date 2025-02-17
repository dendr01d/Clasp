using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Text
{
    public enum TokenType
    {
        Whitespace,
        Comment,

        Symbol,
        Boolean,

        //Number,
        DecReal,
        BinInteger,
        OctInteger,
        DecInteger,
        HexInteger,

        Character,
        String,

        OpenListParen,
        OpenVecParen,
        ClosingParen,

        Quote,
        Quasiquote,
        Unquote,
        UnquoteSplice,

        Syntax,
        QuasiSyntax,
        Unsyntax,
        UnsyntaxSplice,

        ModuleFlag,

        EOF,
        DotOperator,
        Undefined,

        Malformed
    }
}
