using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ClaspCompiler.Tokens
{
    internal sealed class TokenStream : IPrintable, IEnumerable<Token>
    {
        private readonly IEnumerable<Token> _stream;

        public TokenStream(IEnumerable<Token> stream)
        {
            _stream = stream;
        }

        public override string ToString() => string.Join(' ', _stream);

        public void Print(TextWriter writer, int indent) => writer.Write(ToString());

        public IEnumerator<Token> GetEnumerator() => _stream.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_stream).GetEnumerator();
    }
}
