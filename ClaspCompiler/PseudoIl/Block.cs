using System.Collections;

namespace ClaspCompiler.PseudoIl
{
    internal class Block : IPrintable, IEnumerable<IInstruction>
    {
        public readonly string Info;
        private readonly List<IInstruction> _instructions;
        public Block(string info, params IInstruction[] instrs)
        {
            Info = info;
            _instructions = instrs.ToList();
        }
        public override string ToString() => $"({string.Join(' ', _instructions)})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            foreach (IInstruction instr in _instructions)
            {
                writer.WriteLineIndent(indent);
                writer.Write(instr, indent);
            }
            writer.Write(')');
        }

        public IEnumerator<IInstruction> GetEnumerator()
        {
            return ((IEnumerable<IInstruction>)_instructions).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_instructions).GetEnumerator();
        }

        public void Add(IInstruction instr) => _instructions.Add(instr);
    }
}
