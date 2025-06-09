using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClaspCompiler.CompilerData;

namespace ClaspCompiler.IntermediateCil
{
    internal sealed class Block : IPrintable, IEnumerable<Instruction>
    {
        public Label Label { get; init; }
        public readonly List<Instruction> Instructions;
        public HashSet<RegLocal> LocalRegisters { get; init; }

        public HashSet<TempVar>[] LivenessStatus { get; init; }
        public Dictionary<TempVar, HashSet<TempVar>> InterferenceGraph { get; init; }

        public Block(Label label, IEnumerable<Instruction> instrs,
            IEnumerable<HashSet<TempVar>>? liveness = null,
            IDictionary<TempVar, HashSet<TempVar>>? interference = null)
        {
            Label = label;

            Instructions = new();
            LocalRegisters = new();
            //ParamRegisters = new();

            foreach (Instruction instr in instrs) Add(instr);

            LivenessStatus = liveness?.ToArray() ?? [];
            InterferenceGraph = interference?.ToDictionary() ?? new();
        }

        public bool BreaksLine => true;
        public string AsString => $"({Label} . ({string.Join(' ', Instructions)}))";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            writer.Write(Label, indent);
            writer.WriteLineIndent(" .", indent);

            writer.WriteIndenting('[', ref indent);

            writer.WriteLineByLine(Instructions, indent);

            writer.Write("])");
        }
        public override string ToString() => AsString;


        public IEnumerator<Instruction> GetEnumerator() => throw new NotSupportedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
        public void Add(Instruction instr)
        {
            Instructions.Add(instr);

            if (instr.Operand is RegLocal loc) LocalRegisters.Add(loc);
            //if (instr.Operand is RegParam par) ParamRegisters.Add(par);
        }
    }
}
