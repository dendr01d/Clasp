using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Terms
{
    internal abstract class SyntaxRule : Atom
    {

    }

    internal class MatchPattern : SyntaxRule
    {
        public readonly Pair LiteralSymbols;
        public readonly Expression Pattern;

        public MatchPattern(Pair literals, Expression pattern)
        {
            LiteralSymbols = literals;
            Pattern = pattern;
        }
    }

    internal class TemplatePattern : SyntaxRule
    {
        public readonly Environment DefinitionEnv;
        public readonly Expression Pattern;

        public TemplatePattern(Environment defEnv, Expression pattern)
        {
            DefinitionEnv = defEnv;
            Pattern = pattern;
        }
    }
}
