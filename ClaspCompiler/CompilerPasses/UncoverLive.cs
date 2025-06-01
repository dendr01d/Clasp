using System.Collections.Immutable;

using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateLocLang;
using ClaspCompiler.IntermediateLocLang.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class UncoverLive
    {
        public static ProgLoc0 Execute(ProgLoc0 program)
        {

            var updatedBlocks = program.LabeledBlocks
                .ToDictionary(x => x.Key, x => AnalyzeBlock(x.Value));

            return new ProgLoc0(program.LocalVariables, updatedBlocks);
        }

        private static BinaryBlock AnalyzeBlock(BinaryBlock block)
        {
            List<ImmutableHashSet<Var>> livenessChain = [];
            ImmutableHashSet<Var> liveSet = [];

            livenessChain.Add(liveSet);

            foreach (BinaryInstruction instr in block.Instructions.Reverse())
            {
                if (instr.Destination is not null)
                {
                    liveSet = liveSet.Remove(instr.Destination);
                }

                if (instr.Argument is Var v)
                {
                    liveSet = liveSet.Add(v);
                }

                livenessChain.Add(liveSet);
            }

            return new BinaryBlock(livenessChain.AsEnumerable().Reverse(), block.Instructions);
        }
    }
}
