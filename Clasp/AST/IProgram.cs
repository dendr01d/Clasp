using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.InterLangs;

namespace Clasp.AST
{
    internal interface IProgram<T> : INode<T>
        where T : InterLang<T>
    {
    }
}
