using Clasp.InterLangs;

namespace Clasp.AST
{
    /// <summary>
    /// Represents an AST node with one or more branches.
    /// </summary>
    /// <typeparam name="T">The language in which the program is written.</typeparam>
    internal interface INonTerminal<T> : INode<T>
        where T : InterLang<T>
    {
    }
}
