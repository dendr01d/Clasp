using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Exceptions
{
    public abstract class LexerException : ClaspException, ISourceTraceable
    {
        public SourceCode Location { get; private set; }

        private LexerException(SourceCode loc, string format, params object[] args) : base(format, args)
        {
            Location = loc;
        }

        public class MalformedInput : LexerException
        {
            internal MalformedInput(Token token) : base(
                token.Location,
                "Malformed input on line {0} beginning at column {1}: {2}",
                token.Location.LineNumber,
                token.Location.Column,
                token.Text)
            { }
        }
    }
}
