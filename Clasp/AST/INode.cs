using Clasp.InterLangs;

namespace Clasp.AST
{
    /// <summary>
    /// The semantic representation of a program's tree structure.
    /// </summary>
    /// <typeparam name="T">The language in which the program is written.</typeparam>
    internal interface INode<T>
        where T : InterLang<T>
    {
        string ToString();
    }
}
