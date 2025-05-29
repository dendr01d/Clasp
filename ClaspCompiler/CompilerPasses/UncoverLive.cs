using System.Collections.Immutable;

using ClaspCompiler.IntermediateStackLang.Abstract;
using ClaspCompiler.IntermediateStackLang;

namespace ClaspCompiler.CompilerPasses
{
    internal static class UncoverLive
    {
        public static ProgStack0 Execute(ProgStack0 program)
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

            foreach (IStackInstr instr in block.Reverse())
            {
                liveSet = new HashSet<IMem>(liveSet
                    .Except(AnalyzeWrites(instr))
                    .Union(AnalyzeReads(instr)))
                    .ToImmutableHashSet();

                livenessChain.Add(liveSet);
            }

            return new Block(livenessChain.AsEnumerable().Reverse(), block);
        }

        private static HashSet<IMem> AnalyzeWrites(IStackInstr instr)
        {
            if (instr is Instruction rInstr
                && rInstr.Operator == StackOp.Store)
            {
                if (rInstr.Operand is IMem mem)
                {
                    return [mem];
                }
            }

            return [];
        }

        private static HashSet<IMem> AnalyzeReads(IStackInstr instr)
        {
            if (instr is Instruction rInstr
                && rInstr.Operator == StackOp.Load)
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
