using System.Collections.Immutable;

using ClaspCompiler.PseudoIl;

namespace ClaspCompiler.CompilerPasses
{
    internal static class AnalyzeLiveness
    {
        public static ProgIl0 Execute(ProgIl0 program)
        {

            var updatedBlocks = program.LabeledBlocks
                .ToDictionary(x => x.Key, x => AnalyzeBlock(x.Value));

            return new ProgIl0(program.LocalVariables, updatedBlocks);
        }

        private static Block AnalyzeBlock(Block block)
        {
            List<ImmutableHashSet<IMem>> livenessChain = [];
            ImmutableHashSet<IMem> liveSet = [];

            livenessChain.Add(liveSet);

            foreach (IInstruction instr in block.Reverse())
            {
                liveSet = new HashSet<IMem>(liveSet
                    .Except(AnalyzeWrites(instr))
                    .Union(AnalyzeReads(instr)))
                    .ToImmutableHashSet();

                livenessChain.Add(liveSet);
            }

            return new Block(livenessChain.AsEnumerable().Reverse(), block);
        }

        private static HashSet<IMem> AnalyzeWrites(IInstruction instr)
        {
            if (instr is Instruction rInstr
                && rInstr.Operator == PseudoOp.Store)
            {
                if (rInstr.Operand is IMem mem)
                {
                    return [mem];
                }
            }

            return [];
        }

        private static HashSet<IMem> AnalyzeReads(IInstruction instr)
        {
            if (instr is Instruction rInstr
                && rInstr.Operator == PseudoOp.Load)
            {
                if (rInstr.Operand is IMem mem)
                {
                    return [mem];
                }
            }

            return [];
        }
    }
}
