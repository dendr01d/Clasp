using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateStackLang.Abstract;
using ClaspCompiler.IntermediateStackLang;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.IntermediateLocLang;

namespace ClaspCompiler.CompilerPasses
{
    //internal static class AssignHomes
    //{
    //    public static ProgStack0 Execute(ProgLoc0 program)
    //    {
    //        Dictionary<Var, Var> map = [];
    //        Dictionary<Label, Block> blocks = [];

    //        foreach (var pair in program.LabeledBlocks)
    //        {
    //            blocks[pair.Key] = ReplaceInBlock(pair.Value, map);
    //        }

    //        Dictionary<Var, SchemeType> localVars = [];

    //        foreach (var pair in program.LocalVariables)
    //        {
    //            if (pair.Key is Var var && map.TryGetValue(var, out Var? newMem))
    //            {
    //                localVars[newMem] = pair.Value;
    //            }
    //        }

    //        return new ProgIl0(localVars, blocks);
    //    }

    //    private static Block ReplaceInBlock(Block block, Dictionary<Var, Var> map)
    //    {
    //        return new Block(block.Liveness,
    //            block.Select(x => ReplaceInInstruction(x, map)).ToArray());
    //    }

    //    private static IStackInstr ReplaceInInstruction(IStackInstr instr, Dictionary<Var, Var> map)
    //    {
    //        if (instr.Operand is Var var)
    //        {
    //            if (!map.TryGetValue(var, out Var? mem))
    //            {
    //                mem = new LocalVar(map.Count);
    //                map.Add(var, mem);
    //            }
    //            return new Instruction(instr.Operator, mem, instr.LineLabel);
    //        }

    //        return instr;
    //    }
    //}
}
