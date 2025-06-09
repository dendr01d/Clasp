using ClaspCompiler.IntermediateCil;
using ClaspCompiler.IntermediateCil.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class UncoverLive
    {
        public static Prog_Cil Execute(Prog_Cil program)
        {
            var newBlocks = program.LabeledBlocks
                .ToDictionary(x => x.Key, x => UncoverInBlock(x.Value));

            return new Prog_Cil(newBlocks);
        }

        private static Block UncoverInBlock(Block block)
        {
            IEnumerable<HashSet<TempVar>> liveness = UncoverInInstructions(block.Instructions).Reverse();

            return new Block(block.Label, block.Instructions, liveness);
        }

        private static IEnumerable<HashSet<TempVar>> UncoverInInstructions(IEnumerable<Instruction> instrs)
        {
            HashSet<TempVar> liveVars = new();

            yield return liveVars;

            foreach (Instruction instr in instrs.Reverse())
            {
                liveVars = (liveVars.Except(WriteSet(instr))).Union(ReadSet(instr)).ToHashSet();

                yield return liveVars;
            }
        }

        private static HashSet<TempVar> WriteSet(Instruction instr)
        {
            return instr.Operator == CilOp.Store
                && instr.Operand is TempVar v
                ? [v]
                : [];
        }

        private static HashSet<TempVar> ReadSet(Instruction instr)
        {
            return instr.Operator == CilOp.Load
                && instr.Operand is TempVar v
                ? [v]
                : [];
        }
    }
}
