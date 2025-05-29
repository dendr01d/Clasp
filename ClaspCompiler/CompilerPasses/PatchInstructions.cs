using ClaspCompiler.IntermediateStackLang.Abstract;
using ClaspCompiler.IntermediateStackLang;

namespace ClaspCompiler.CompilerPasses
{
    internal static class PatchInstructions
    {
        public static ProgStack0 Execute(ProgStack0 program)
        {
            var patchedBlocks = program.LabeledBlocks
                .ToDictionary(x => x.Key, x => PatchBlock(x.Value));

            return new ProgStack0(patchedBlocks);
        }

        private static Block PatchBlock(Block block)
        {
            List<IStackInstr> patchedInstrs = block.ToList();

            for (int i = 1; i < block.Count; ++i)
            {
                IStackInstr first = block[i - 1];
                IStackInstr second = block[i];

                if (first.Operator == StackOp.Load
                    && second.Operator == StackOp.Store
                    && first.Operand == second.Operand)
                {
                    patchedInstrs.RemoveAt(i);
                    patchedInstrs.RemoveAt(i - 1);
                }
                else if (first.Operator == StackOp.Store
                    && second.Operator == StackOp.Load
                    && first.Operand == second.Operand)
                {
                    patchedInstrs.RemoveAt(i);
                    patchedInstrs.RemoveAt(i - 1);
                }
            }

            return new Block(block.Liveness, patchedInstrs);
        }

    }
}
