using Clasp.AST;
using Clasp.InterLangs;

namespace Clasp.Translaters
{
    /// <summary>
    /// Represents a transformer that takes a program in one representation and
    /// translates it to an equivalent program written in another.
    /// </summary>
    /// <typeparam name="I">The representation of the input program.</typeparam>
    /// <typeparam name="O">The representation of the output program. May be the same as <see cref="I"/>.</typeparam>
    internal abstract class Translater<I, O>
        where I : InterLang<I>
        where O : InterLang<O>
    {
        public abstract O Translate(I input);
    }
}
