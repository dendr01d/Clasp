using Clasp.AST;

namespace Clasp.InterLangs
{
    /// <summary>
    /// The metasyntactic representation of a structurally-recursive programming language.
    /// </summary>
    internal abstract class InterLang<T> : INode<T>
        where T : InterLang<T>
    {
        public readonly string Language;

        protected InterLang(string language)
        {
            Language = language;
        }

        public abstract override string ToString();
    }
}
