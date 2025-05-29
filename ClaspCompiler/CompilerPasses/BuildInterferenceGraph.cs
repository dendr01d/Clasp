using ClaspCompiler.IntermediateStackLang;
using ClaspCompiler.IntermediateStackLang.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal class BuildInterferenceGraph
    {
        public static ProgStack0 Execute(ProgStack0 program)
        {
            Dictionary<IMem, HashSet<IMem>> conflicts = [];

            foreach (IMem mem in program.LocalVariables.Keys)
            {
                conflicts.Add(mem, []);
            }

            foreach (var pair in program.LabeledBlocks)
            {
                GraphBlock(pair.Value, conflicts);
            }

            return new ProgIl0(conflicts, program.LocalVariables, program.LabeledBlocks);
        }

        private static void GraphBlock(Block block, Dictionary<IMem, HashSet<IMem>> graph)
        {
            for (int i = 0; i < block.Count; ++i)
            {
                if (block[i].Operator == StackOp.Store
                    && block[i].Operand is IMem mem)
                {
                    foreach (IMem other in block.Liveness[i + 1].Except([mem]))
                    {
                        GraphInterference(mem, other, graph);
                    }
                }
            }
        }

        private static void GraphInterference(IMem nodeA, IMem nodeB, Dictionary<IMem, HashSet<IMem>> graph)
        {
            graph[nodeA].Add(nodeB);
            graph[nodeB].Add(nodeA);
        }
    }
}
