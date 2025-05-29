using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData;
using ClaspCompiler.IntermediateStackLang;
using ClaspCompiler.IntermediateStackLang.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class AllocateHomes
    {
        public static ProgStack0 Execute(ProgStack0 program)
        {
            Dictionary<IMem, IMem> map = CreateMapping(program.Conflicts ?? []);

            Dictionary<Label, Block> remappedBlocks = program.LabeledBlocks
                .ToDictionary(x => x.Key, x => ReplaceInBlock(x.Value, map));

            //Dictionary<IMem, TypeName> localVars = (program.LocalVariables ?? [])
            //    .ToDictionary(x => map[x.Key], x => x.Value);

            //Dictionary<IMem, HashSet<IMem>> conflicts = (program.Conflicts ?? [])
            //    .ToDictionary(
            //        x => map[x.Key],
            //        x => x.Value.Select(y => map[y]).ToHashSet());

            return new ProgStack0(remappedBlocks);
        }

        private static Dictionary<IMem, IMem> CreateMapping(Dictionary<IMem, HashSet<IMem>> interferenceGraph)
        {
            HashSet<IMem> remainingVars = interferenceGraph.Keys.ToHashSet();
            Dictionary<IMem, HashSet<int>> saturations = remainingVars.ToDictionary(x => x, x => new HashSet<int>());
            Dictionary<IMem, int> assignedColors = [];

            while (remainingVars.Count > 0
                && remainingVars.MaxBy(x => saturations[x].Count) is IMem next)
            {
                HashSet<int> interferences = assignedColors
                    .Where(x => interferenceGraph[next].Contains(x.Key))
                    .Select(x => x.Value)
                    .ToHashSet();

                int i = 0;
                while (interferences.Contains(i))
                {
                    ++i;
                }

                assignedColors.Add(next, i);
                remainingVars.Remove(next);

                foreach (Var interfering in interferenceGraph[next])
                {
                    saturations[interfering].Add(i);
                }
            }

            return assignedColors.ToDictionary(x => x.Key, x => new LocalVar(x.Value) as IMem);
        }

        private static Block ReplaceInBlock(Block block, Dictionary<IMem, IMem> map)
        {
            return new Block(block.Liveness,
                block.Select(x => ReplaceInInstruction(x, map)));
        }

        private static IStackInstr ReplaceInInstruction(IStackInstr instr, Dictionary<IMem, IMem> map)
        {
            if (instr.Operand is IMem oldMem)
            {
                if (!map.TryGetValue(oldMem, out IMem? newMem))
                {
                    newMem = new LocalVar(map.Count);
                    map.Add(oldMem, newMem);
                }
                return new Instruction(instr.Operator, newMem, instr.LineLabel);
            }

            return instr;
        }
    }
}
