using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.AbstractSyntax;

namespace Clasp.Data.Terms
{
    internal abstract class Term
    {
        public static implicit operator AstNode(Term t) => new Fixed(t);
    }
}
