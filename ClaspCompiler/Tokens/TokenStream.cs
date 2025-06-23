using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ClaspCompiler.Tokens
{
    internal sealed class TokenStream : IPrintable, IEnumerable<Token>
    {
        private readonly IEnumerable<Token> _stream;

        public TokenStream(IEnumerable<Token> stream) => _stream = stream;

        public bool BreaksLine => false;
        public string AsString => string.Join(' ', _stream);
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
        public override string ToString() => AsString;

        public IEnumerator<Token> GetEnumerator() => _stream.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _stream.GetEnumerator();
    }
}
