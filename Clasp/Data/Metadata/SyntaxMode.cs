using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Metadata
{
    internal enum SyntaxMode
    {
        Expression,
        InternalDefinition,
        Module,
        TopLevel,
        Partial
    }
}
