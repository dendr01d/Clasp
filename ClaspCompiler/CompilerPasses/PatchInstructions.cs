namespace ClaspCompiler.CompilerPasses
{
    //internal static class PatchInstructions
    //{
    //    public static ProgStack0 Execute(ProgLoc0 program)
    //    {
    //        var patchedBlocks = program.LabeledBlocks
    //            .ToDictionary(x => x.Key, x => PatchBlock(x.Value, program.LocalMap));

    //        return new ProgStack0(patchedBlocks);
    //    }

    //    private static UnaryBlock PatchBlock(BinaryBlock block, Dictionary<Var, int> map)
    //    {
    //        int stackDepth = 0;
    //        List<UnaryInstruction> newInstructions = new();

    //        foreach (BinaryInstruction bina in block.Instructions)
    //        {
    //            newInstructions.AddRange(PatchInstruction(bina, map, ref stackDepth));
    //        }

    //        return new UnaryBlock(newInstructions);
    //    }

    //    private static IEnumerable<UnaryInstruction> PatchInstruction(BinaryInstruction instr, Dictionary<Var, int> map, ref int stackDepth)
    //    {
    //        IStackArg? arg = PatchArg(instr.Argument, map);
    //        IRegister? dest = PatchReg(instr.Destination, map);

    //        switch(instr.Operator)
    //        {
    //            case LocOp.MOVE:
    //                if (arg != dest)
    //                {
    //                    return [
    //                        new UnaryInstruction(StackOp.Load, arg),
    //                        new UnaryInstruction(StackOp.Store, dest)
    //                        ];
    //                }
    //                return [];

    //            case LocOp.ADD:
    //                return [
    //                    new UnaryInstruction(StackOp.Load, arg),
    //                    new UnaryInstruction(StackOp.Load, dest),
    //                    new UnaryInstruction(StackOp.Add),
    //                    new UnaryInstruction(StackOp.Store, dest)
    //                    ];

    //            case LocOp.SUB:
    //                stackDepth += 1;
    //                return [
    //                    new UnaryInstruction(StackOp.Load, arg),
    //                    new UnaryInstruction(StackOp.Load, dest),
    //                    new UnaryInstruction(StackOp.Sub),
    //                    new UnaryInstruction(StackOp.Store, dest)
    //                    ];

    //            case LocOp.NEG:
    //                stackDepth += 1;
    //                return [
    //                    new UnaryInstruction(StackOp.Load, arg),
    //                    new UnaryInstruction(StackOp.Neg),
    //                    new UnaryInstruction(StackOp.Store, dest)
    //                    ];

    //            case LocOp.READ:
    //                return [
    //                    new UnaryInstruction(StackOp.Call, new Label("string [System.Console]System.Console::ReadLine()")),
    //                    new UnaryInstruction(StackOp.Call, new Label("int32 [System.Runtime]System.Int32::Parse(string)")),
    //                    new UnaryInstruction(StackOp.Store, dest)
    //                    ];

    //            case LocOp.RETURN:
    //                return [
    //                    new UnaryInstruction(StackOp.Load, arg),
    //                    new UnaryInstruction(StackOp.Return)
    //                    ];
    //        }

    //        throw new Exception($"Can't parse unknown instruction: {instr}");
    //    }

    //    private static IStackArg? PatchArg(ILocArg? arg, Dictionary<Var, int> map)
    //    {
    //        if (arg is null)
    //        {
    //            return null;
    //        }
    //        else if (arg is Var v)
    //        {
    //            return PatchReg(v, map);
    //        }
    //        else if (arg is IStackArg lit)
    //        {
    //            return lit;
    //        }

    //        throw new Exception($"Can't patch argument of unknown type: {arg}");
    //    }

    //    private static IRegister? PatchReg(Var? v, Dictionary<Var, int> map)
    //    {
    //        if (v is null)
    //        {
    //            return null;
    //        }
    //        else if (map.TryGetValue(v, out int reg))
    //        {
    //            return new LocalVar(reg);
    //        }
    //        else
    //        {
    //            throw new Exception($"Can't patch unmapped local variable: {v}");
    //        }
    //    }

    //    //private static BlockLoc ReplaceInBlock(BlockLoc block, Dictionary<Var, Var> map)
    //    //{
    //    //    return new Block(block.Liveness,
    //    //        block.Select(x => ReplaceInInstruction(x, map)));
    //    //}

    //    //private static Instruction ReplaceInInstruction(Instruction instr, Dictionary<Var, Var> map)
    //    //{
    //    //    if (instr.Operand is Var oldMem)
    //    //    {
    //    //        if (!map.TryGetValue(oldMem, out Var? newMem))
    //    //        {
    //    //            newMem = new LocalVar(map.Count);
    //    //            map.Add(oldMem, newMem);
    //    //        }
    //    //        return new Instruction(instr.Operator, newMem, instr.LineLabel);
    //    //    }

    //    //    return instr;
    //    //}
    //}
}
