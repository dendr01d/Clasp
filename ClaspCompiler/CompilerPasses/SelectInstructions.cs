namespace ClaspCompiler.CompilerPasses
{
    //internal sealed class SelectInstructions
    //{
    //    public static ProgLoc0 Execute(ProgC0 program)
    //    {
    //        Dictionary<Label, BinaryBlock> labeledBlocks = [];

    //        foreach (var pair in program.LabeledTails)
    //        {
    //            BinaryBlock block = new(SelectTail(pair.Value));

    //            labeledBlocks.Add(pair.Key, block);
    //        }

    //        //var localVars = program.LocalVariables.ToDictionary(x => x.Key, x => x.Value);

    //        return new ProgLoc0(program.LocalVariables, labeledBlocks);
    //    }

    //    private static IEnumerable<BinaryInstruction> SelectTail(ITail tail)
    //    {
    //        if (tail is Sequence seq)
    //        {
    //            return SelectStatement(seq.Statement)
    //                .Concat(SelectTail(seq.Tail));
    //        }
    //        else if (tail is Return ret)
    //        {
    //            return [new BinaryInstruction(LocOp.RETURN, SelectArgument(ret.Value), null)];
    //        }

    //        throw new Exception($"Can't select instructions from tail: {tail}");
    //    }

    //    private static IEnumerable<BinaryInstruction> SelectStatement(IStatement stmt)
    //    {
    //        if (stmt is Assignment asmt)
    //        {
    //            if (asmt.Value is ICpsArg arg)
    //            {
    //                yield return new BinaryInstruction(LocOp.MOVE, SelectArgument(arg), asmt.Variable);
    //                yield break;
    //            }
    //            else if (asmt.Value is ICpsApp app)
    //            {
    //                ILocArg[] args = app.Arguments.Select(SelectArgument).ToArray();

    //                switch(app.Operator)
    //                {
    //                    case "read":
    //                        yield return new BinaryInstruction(LocOp.READ, null, asmt.Variable);
    //                        yield break;

    //                    case "-":
    //                        yield return new BinaryInstruction(LocOp.NEG, args[0], asmt.Variable);
    //                        yield break;

    //                    case "+":
    //                        yield return new BinaryInstruction(LocOp.MOVE, args[0], asmt.Variable);
    //                        yield return new BinaryInstruction(LocOp.ADD, args[1], asmt.Variable);
    //                        yield break;
    //                }

    //                throw new Exception($"Can't select instructions for application of unknown operator: {app.Operator}");
    //            }
    //            else if (asmt.Value is ILocArg locArg)
    //            {
    //                yield return new BinaryInstruction(LocOp.MOVE, locArg, asmt.Variable);
    //            }
    //        }

    //        throw new Exception($"Can't select instructions from statement: {stmt}");
    //    }

    //    private static ILocArg SelectArgument(ICpsArg arg)
    //    {
    //        if (arg is Var var)
    //        {
    //            return var;
    //        }
    //        else if (arg is IValue imm)
    //        {
    //            return imm;
    //        }

    //        throw new Exception($"Can't select unknown argument type: {arg}");
    //    }
    //}
}
