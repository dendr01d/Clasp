using Clasp.Data.Metadata;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Exceptions
{
    public abstract class ReaderException : ClaspException
    {
        private ReaderException(string format, params object?[] args) : base(format, args) { }

        public class EmptyTokenStream : ReaderException
        {
            internal EmptyTokenStream() : base(
                "The reader received an empty token stream.")
            { }
        }

        public class AmbiguousParenthesis : ReaderException
        {
            internal AmbiguousParenthesis(bool opening) : base(
                "The reader counted an extra {0} parenthesis, but was unable to determine where it is.",
                opening ? "opening" : "closing")
            { }
        }

        public class UnmatchedParenthesis : ReaderException, ISourceTraceable
        {
            public SourceCode Location { get; private set; }

            internal UnmatchedParenthesis(Token errantParenToken) : base(
                "The reader found an {0} parenthesis on line {1}, column {2}.",
                errantParenToken.TType == TokenType.ClosingParen ? "un-opened" : "un-closed",
                errantParenToken.Location.LineNumber,
                errantParenToken.Location.Column)
            {
                Location = errantParenToken.Location;
            }
        }

        public class UnexpectedToken : ReaderException, ISourceTraceable
        {
            public SourceCode Location { get; private set; }

            internal UnexpectedToken(Token errantToken) : base(
                "Unexpected token {0} at line {1}, column {2}.",
                errantToken,
                errantToken.Location.LineNumber,
                errantToken.Location.Column)
            {
                Location = errantToken.Location;
            }
        }

        public class ExpectedToken : ReaderException, ISourceTraceable
        {
            public SourceCode Location { get; private set; }

            internal ExpectedToken(TokenType expectedType, Token receivedToken, Token prevToken) : base(
                "Expected {0} token to follow after {1} on line {2}, column {3}.",
                expectedType,
                prevToken,
                prevToken.Location.LineNumber,
                prevToken.Location.Column)
            {
                Location = receivedToken.Location;
            }
        }

        public class ExpectedListEnd : ReaderException, ISourceTraceable
        {
            public SourceCode Location { get; private set; }

            internal ExpectedListEnd(Token receivedToken, Token previous) : base(
                "Expected {0} token to complete list structure following {1} on line {2}, column {3}.",
                TokenType.ClosingParen,
                previous,
                previous.Location.LineNumber,
                previous.Location.Column)
            {
                Location = receivedToken.Location;
            }
        }

        public class UnhandledToken : ReaderException, ISourceTraceable
        {
            public SourceCode Location { get; private set; }

            internal UnhandledToken(Token errantToken) : base(
                "Token {0} of type {1} (on line {2}, column {3}) is unhandled by CLASP at this time :)",
                errantToken,
                errantToken.TType,
                errantToken.Location.LineNumber,
                errantToken.Location.Column)
            {
                Location = errantToken.Location;
            }
        }

        public class InvalidModuleForm : ReaderException
        {
            internal InvalidModuleForm(Syntax stx) : base(
                "Syntax doesn't comprise a module as expected: {0}",
                stx)
            { }
        }
    }
}
