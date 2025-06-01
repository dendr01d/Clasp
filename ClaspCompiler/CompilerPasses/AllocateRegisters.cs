using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateLocLang;

namespace ClaspCompiler.CompilerPasses
{
    internal static class AllocateRegisters
    {
        public static ProgLoc0 Execute(ProgLoc0 program)
        {
            Dictionary<Var, int> map = CreateMapping(program.Conflicts);

            //Dictionary<Label, Block> remappedBlocks = program.LabeledBlocks
            //    .ToDictionary(x => x.Key, x => ReplaceInBlock(x.Value, map));

            //Dictionary<Var, TypeName> localVars = (program.LocalVariables ?? [])
            //    .ToDictionary(x => map[x.Key], x => x.Value);

            //Dictionary<Var, HashSet<Var>> conflicts = (program.Conflicts ?? [])
            //    .ToDictionary(
            //        x => map[x.Key],
            //        x => x.Value.Select(y => map[y]).ToHashSet());

            return new ProgLoc0(map, program.Conflicts, program.LocalVariables, program.LabeledBlocks);
        }

        private static Dictionary<Var, int> CreateMapping(Dictionary<Var, HashSet<Var>> interferenceGraph)
        {
            HashSet<Var> remainingVars = interferenceGraph.Keys.ToHashSet();
            Dictionary<Var, HashSet<int>> saturations = remainingVars.ToDictionary(x => x, x => new HashSet<int>());
            Dictionary<Var, int> assignedColors = [];

            while (remainingVars.Count > 0
                && remainingVars.MaxBy(x => saturations[x].Count) is Var next)
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

            //return assignedColors.ToDictionary(x => x.Key, x => new LocalVar(x.Value) as Var);
            return assignedColors;
        }
    }
}
