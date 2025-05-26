using System;
using System.Collections;
using System.Collections.Immutable;

namespace ClaspCompiler.PseudoIl
{
    internal class Block : IPrintable, IList<IInstruction>, IEnumerable<IInstruction>
    {
        public readonly List<ImmutableHashSet<IMem>> Liveness;
        private readonly List<IInstruction> _instructions;
        public Block(IEnumerable<ImmutableHashSet<IMem>> liveness, IEnumerable<IInstruction> instrs)
        {
            Liveness = liveness.ToList();
            _instructions = instrs.ToList();
        }

        private static string LivenessToString(ImmutableHashSet<IMem> liveMems)
        {
            return string.Format("   ; {{{0}}} ",
                string.Join(", ", liveMems.Select(x => x.ToString())));
        }

        public override string ToString() => $"({string.Join(' ', _instructions)})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            if (_instructions.Any())
            {
                int padding = -25;
                string comment = Liveness.Count == 0 ? string.Empty : LivenessToString(Liveness.Skip(1).First());
                writer.WriteWithComment(_instructions.First(), padding, comment);

                for (int i = 1; i < _instructions.Count; ++i)
                {
                    writer.WriteLineIndent(indent);
                    comment = Liveness.Count == 0 ? string.Empty : LivenessToString(Liveness[i - 1]);
                    writer.WriteWithComment(_instructions[i], padding, comment);
                }

                writer.WriteLineIndent(indent);
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


        #region IList Implementation
        public int IndexOf(IInstruction item)
        {
            return ((IList<IInstruction>)_instructions).IndexOf(item);
        }

        public void Insert(int index, IInstruction item)
        {
            ((IList<IInstruction>)_instructions).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<IInstruction>)_instructions).RemoveAt(index);
        }

        public IInstruction this[int index] { get => ((IList<IInstruction>)_instructions)[index]; set => ((IList<IInstruction>)_instructions)[index] = value; }

        public void Clear()
        {
            ((ICollection<IInstruction>)_instructions).Clear();
        }

        public bool Contains(IInstruction item)
        {
            return ((ICollection<IInstruction>)_instructions).Contains(item);
        }

        public void CopyTo(IInstruction[] array, int arrayIndex)
        {
            ((ICollection<IInstruction>)_instructions).CopyTo(array, arrayIndex);
        }

        public bool Remove(IInstruction item)
        {
            return ((ICollection<IInstruction>)_instructions).Remove(item);
        }

        public int Count => ((ICollection<IInstruction>)_instructions).Count;

        public bool IsReadOnly => ((ICollection<IInstruction>)_instructions).IsReadOnly;
        #endregion
    }
}
