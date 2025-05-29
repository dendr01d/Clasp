using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateStackLang.Abstract;
using ClaspCompiler.IntermediateStackLang;
using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class AssignHomes
    {
        public static ProgStack0 Execute(ProgStack0 program)
        {
            Dictionary<Var, IMem> map = [];
            Dictionary<Label, Block> blocks = [];

            foreach (var pair in program.LabeledBlocks)
            {
                blocks[pair.Key] = ReplaceInBlock(pair.Value, map);
            }

            Dictionary<IMem, SchemeType> localVars = [];

            foreach (var pair in program.LocalVariables)
            {
                if (pair.Key is Var var && map.TryGetValue(var, out IMem? newMem))
                {
                    localVars[newMem] = pair.Value;
                }
            }

            return new ProgIl0(localVars, blocks);
        }

        private static Block ReplaceInBlock(Block block, Dictionary<Var, IMem> map)
        {
            return new Block(block.Liveness,
                block.Select(x => ReplaceInInstruction(x, map)).ToArray());
        }

        private static IStackInstr ReplaceInInstruction(IStackInstr instr, Dictionary<Var, IMem> map)
        {
            if (instr.Operand is Var var)
            {
                if (!map.TryGetValue(var, out IMem? mem))
                {
                    mem = new LocalVar(map.Count);
                    map.Add(var, mem);
                }
                return new Instruction(instr.Operator, mem, instr.LineLabel);
            }

            return instr;
        }
    }
}
