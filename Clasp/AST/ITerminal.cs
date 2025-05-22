using Clasp.InterLangs;

namespace Clasp.AST
{
    /// <summary>
    /// Represents a leaf node of a program -- a node with no branches.
    /// </summary>
    /// <typeparam name="T">The language in which the program is written.</typeparam>
    internal interface ITerminal<T, U> : INode<T>
        where T : InterLang<T>
    {
        public U Value { get; }
    }
}
