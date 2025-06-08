namespace ClaspCompiler.CompilerPasses
{
    //internal class BuildInterferenceGraph
    //{
    //    public static ProgLoc0 Execute(ProgLoc0 program)
    //    {
    //        Dictionary<Var, HashSet<Var>> conflicts = [];

    //        foreach (Var mem in program.LocalVariables.Keys)
    //        {
    //            conflicts.Add(mem, []);
    //        }

    //        foreach (var pair in program.LabeledBlocks)
    //        {
    //            GraphBlock(pair.Value, conflicts);
    //        }

    //        return new ProgLoc0(conflicts, program.LocalVariables, program.LabeledBlocks);
    //    }

    //    // Siek, pg 43
    //    private static void GraphBlock(BinaryBlock block, Dictionary<Var, HashSet<Var>> graph)
    //    {
    //        for (int i = 0; i < block.Instructions.Length; ++i)
    //        {
    //            BinaryInstruction instr = block.Instructions[i];

    //            if (instr.Destination is null) continue;

    //            if (instr.Operator == LocOp.MOVE)
    //            {
    //                foreach(Var liveVar in block.Liveness[i + 1])
    //                {
    //                    if (instr.Argument != (ILocArg)liveVar
    //                        && instr.Destination != liveVar)
    //                    {
    //                        GraphInterference(instr.Destination, liveVar, graph);
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                foreach(Var liveVar in block.Liveness[i + 1])
    //                {
    //                    if (instr.Destination != liveVar)
    //                    {
    //                        GraphInterference(instr.Destination, liveVar, graph);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    private static void GraphInterference(Var nodeA, Var nodeB, Dictionary<Var, HashSet<Var>> graph)
    //    {
    //        graph[nodeA].Add(nodeB);
    //        graph[nodeB].Add(nodeA);
    //    }
    //}
}
