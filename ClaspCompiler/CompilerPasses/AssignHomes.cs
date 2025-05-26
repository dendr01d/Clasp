using ClaspCompiler.Common;
using ClaspCompiler.Data;
using ClaspCompiler.PseudoIl;

namespace ClaspCompiler.CompilerPasses
{
    internal static class AssignHomes
    {
        public static ProgIl0 Execute(ProgIl0 program)
        {
            Dictionary<Var, IMem> map = [];
            Dictionary<Label, Block> blocks = [];

            foreach (var pair in program.LabeledBlocks)
            {
                blocks[pair.Key] = ReplaceInBlock(pair.Value, map);
            }

            Dictionary<IMem, TypeName> localVars = [];

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

        private static IInstruction ReplaceInInstruction(IInstruction instr, Dictionary<Var, IMem> map)
        {
            if (instr.Operand is Var var)
            {
                if (!map.TryGetValue(var, out IMem? mem))
                {
                    mem = new LocalMem(map.Count);
                    map.Add(var, mem);
                }
                return new Instruction(instr.Operator, mem, instr.LineLabel);
            }

            return instr;
        }
    }
}
