using ClaspCompiler.IntermediateCil;

namespace ClaspCompiler.CompilerPasses
{
    internal static class AllocateRegisters
    {
        public static Prog_Cil Execute(Prog_Cil program)
        {
            var newBlocks = program.LabeledBlocks
                .ToDictionary(x => x.Key, x => AllocateInBlock(x.Value));

            return new Prog_Cil(newBlocks);
        }

        private static Block AllocateInBlock(Block block)
        {
            Dictionary<TempVar, int> coloring = ColorGraph(block.InterferenceGraph);
            Dictionary<TempVar, RegLocal> localMap = coloring
                .ToDictionary(x => x.Key, x => new RegLocal(x.Value));

            return new Block(block.Label, MapLocalsInInstructions(block.Instructions, localMap));
        }

        private static Dictionary<TempVar, int> ColorGraph(IDictionary<TempVar, HashSet<TempVar>> graph)
        {
            HashSet<TempVar> remainingVars = graph.Keys.ToHashSet();
            Dictionary<TempVar, HashSet<int>> saturations = remainingVars.ToDictionary(x => x, _ => new HashSet<int>());
            Dictionary<TempVar, int> output = new();

            while (remainingVars.Count > 0
                && remainingVars.MaxBy(x => saturations[x].Count) is TempVar next)
            {
                HashSet<int> interferences = output
                    .Where(x => graph[next].Contains(x.Key))
                    .Select(x => x.Value)
                    .ToHashSet();

                int newColor = 0;
                while (interferences.Contains(newColor))
                {
                    ++newColor;
                }

                output.Add(next, newColor);
                remainingVars.Remove(next);

                foreach (TempVar neighbor in graph[next])
                {
                    saturations[neighbor].Add(newColor);
                }
            }

            return output;
        }

        private static IEnumerable<Instruction> MapLocalsInInstructions(IEnumerable<Instruction> instrs, Dictionary<TempVar, RegLocal> map)
        {
            foreach (Instruction instr in instrs)
            {
                if (instr.Operand is TempVar v)
                {
                    yield return new Instruction(instr.Operator, map[v]);
                }
                else
                {
                    yield return instr;
                }
            }
        }
    }
}
