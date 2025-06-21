using ClaspCompiler.SchemeCore.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeCore
{
    internal sealed class Succession : ICoreExp
    {
        public ICoreExp[] Sequents { get; init; }
        public SchemeType Type { get; init; }

        public Succession(params ICoreExp[] sequents)
        {
            Sequents = sequents;
            Type = sequents[^1].Type;
        }

        public bool BreaksLine => Sequents.Length > 1;
        public string AsString => $"(begin {string.Join(' ', Sequents.AsEnumerable())})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication("begin", Sequents, indent);
        public sealed override string ToString() => AsString;
    }
}
