using System.Collections;

using ClaspCompiler.CompilerData;

namespace ClaspCompiler.IntermediateCil
{
    internal sealed class Prog_Cil : IPrintable, IEnumerable<Block>
    {
        public Dictionary<Label, Block> LabeledBlocks { get; init; }
        public HashSet<RegLocal> Locals { get; init; }

        public Prog_Cil(IEnumerable<Block> blocks)
        {
            Locals = new();
            LabeledBlocks = blocks.ToDictionary(x => x.Label, x => x);
        }

        public Prog_Cil(IDictionary<Label, Block> labeledBlocks)
        {
            Locals = new();
            LabeledBlocks = labeledBlocks.ToDictionary();
        }

        public bool BreaksLine => true;
        public string AsString => $"(program {string.Join(' ', LabeledBlocks.Values)})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteLineIndent("(program ()", indent);
            writer.WriteIndenting(" (", ref indent);

            writer.WriteLineByLine(LabeledBlocks.Values, indent);

            writer.Write("))");
        }
        public sealed override string ToString() => AsString;

        public IEnumerator<Block> GetEnumerator() => throw new NotSupportedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
        public void Add(Block block)
        {
            LabeledBlocks.Add(block.Label, block);
            Locals.UnionWith(block.LocalRegisters);
        }
    }
}
