using ClaspCompiler.IntermediateCil;
using ClaspCompiler.IntermediateCil.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class BuildInteferenceGraph
    {
        public static Prog_Cil Execute(Prog_Cil program)
        {
            var newBlocks = program.LabeledBlocks
                .ToDictionary(x => x.Key, x => BuildInBlock(x.Value));

            return new Prog_Cil(newBlocks);
        }

        private static Block BuildInBlock(Block block)
        {
            Dictionary<TempVar, HashSet<TempVar>> graph = new();

            int i = 0;

            foreach (Instruction instr in block.Instructions)
            {
                if (instr.Operator == CilOp.Store
                    && instr.Operand is TempVar v1)
                {
                    foreach (TempVar v2 in block.LivenessStatus[i + 1])
                    {
                        AddInterference(v1, v2, graph);
                    }
                }

                ++i;
            }

            return new Block(block.Label, block.Instructions, block.LivenessStatus, graph);
        }

        private static void AddInterference(TempVar v1, TempVar v2, Dictionary<TempVar, HashSet<TempVar>> graph)
        {
            if (!graph.TryGetValue(v1, out var g1))
            {
                g1 = new();
                graph[v1] = g1;
            }

            if (!graph.TryGetValue(v2, out var g2))
            {
                g2 = new();
                graph[v2] = g2;
            }

            if (v1 != v2)
            {
                g1.Add(v2);
                g2.Add(v1);
            }
        }
    }
}
