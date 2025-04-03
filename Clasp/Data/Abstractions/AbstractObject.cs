using Clasp.Data.Terms;

namespace Clasp.Data.Abstractions
{
    /// <summary>
    /// Represents a single "node" in the abstract syntax tree of an S-Expression.
    /// </summary>
    /// <remarks>
    /// Heavily-inspired by the object hierarchy from "Lisp in Small Pieces" 9.11.1 (pg 345)
    /// </remarks>
    internal abstract class AbstractObject : IAbstractForm
    {
        public abstract override string ToString();
        public abstract ITerm Express();
    }
}
