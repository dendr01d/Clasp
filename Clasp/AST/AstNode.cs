using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.AST
{
    internal abstract class AstNode
    {
        public abstract override string ToString();
    }

    /// <summary>
    /// A "tag" interface indicating that the given class may cause mutative side-effects when evaluated
    /// </summary>
    internal interface IEffective
    {
        public string ToString();
    }

    /// <summary>
    /// A "tag" interface indicating that the given class could also be <see cref="Nil"/>. Poor man's union type.
    /// </summary>
    internal interface INil
    {
        public string ToString();
    }
}
