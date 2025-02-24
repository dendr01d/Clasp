using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Process;

namespace Clasp.Ops
{
    internal static class ProcessOps
    {
        public static Syntax Read(CharString str) => Reader.ReadTokens(Lexer.LexText("Runtime", str.Value));

    }
}
