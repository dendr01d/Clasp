using System.Collections;

using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Body(Definition[] Definitions, ISemCmd[] Commands, ISemExp Value) : ISemSubForm, IEnumerable<ISemAstNode>
    {
        public bool BreaksLine => Definitions.Length > 0 || Commands.Length > 0 || Value.BreaksLine;
        public string AsString => string.Join(' ', Enumerate());
        public void Print(TextWriter writer, int indent) => writer.WriteLineByLine(Enumerate(), indent);
        public sealed override string ToString() => AsString;

        private IEnumerable<ISemAstNode> Enumerate() => ((ISemAstNode[])Definitions).Concat(Commands).Append(Value);
        public IEnumerator<ISemAstNode> GetEnumerator() => Enumerate().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
