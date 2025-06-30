using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCil;
using ClaspCompiler.IntermediateCil.Abstract;
using ClaspCompiler.IntermediateCps;
using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    //internal static class SelectInstructions
    //{
    //    public static Prog_Cil Execute(Prog_Cps program)
    //    {
    //        var blocks = program.LabeledTails.Select(x => SelectBlock(x.Key, x.Value));

    //        return new Prog_Cil(blocks);
    //    }

    //    private static Block SelectBlock(Label label, ITail tail)
    //    {
    //        return new Block(label, SelectTail(tail));
    //    }

    //    private static IEnumerable<Instruction> SelectTail(ITail tail)
    //    {
    //        return tail switch
    //        {
    //            Sequence seq => SelectStatement(seq.Statement).Concat(SelectTail(seq.Tail)),
    //            Return ret => SelectReturning(ret),
    //            Conditional cond => SelectBranch(cond.Condition, cond.Consequent, cond.Alternative),
    //            GoTo jump => [new Instruction(CilOp.Br, jump.Label)],
    //            _ => throw new Exception($"Can't select instructions from unknown tail form: {tail}")
    //        };
    //    }

    //    private static IEnumerable<Instruction> SelectReturning(Return ret)
    //    {
    //        return SelectExpression(ret.Value)
    //            .Append(new Instruction(CilOp.Return));
    //    }

    //    private static IEnumerable<Instruction> SelectStatement(IStatement stmt)
    //    {
    //        return stmt switch
    //        {
    //            Assignment assgn => SelectAssignment(assgn.Variable, assgn.Value),
    //            SideEffect sfx => SelectExpression(sfx.Value),
    //            _ => throw new Exception($"Can't select instructions from unknown statement form: {stmt}")
    //        };
    //    }

    //    private static IEnumerable<Instruction> SelectAssignment(VarBase var, ICpsExp value)
    //    {
    //        return SelectExpression(value)
    //            .Append(new Instruction(CilOp.Store, new TempVar(var)));
    //    }

    //    private static IEnumerable<Instruction> SelectBranch(ICpsExp cond, ITail consq, ITail alt)
    //    {
    //        GoTo br1 = ExpectGoTo(consq);
    //        GoTo br2 = ExpectGoTo(alt);

    //        if (cond is Application app && app.Operator.IsComparison())
    //        {
    //            CilOp jump = app.Operator switch
    //            {
    //                PrimitiveOperator.Eq => CilOp.BrEq,
    //                PrimitiveOperator.Lt => CilOp.BrLt,
    //                PrimitiveOperator.LtE => CilOp.BrLeq,
    //                PrimitiveOperator.Gt => CilOp.BrGt,
    //                PrimitiveOperator.GtE => CilOp.BrGeq,
    //                _ => throw new Exception($"Unknown comparison operator: {app.Operator}")
    //            };

    //            return SelectExpression(app.Arguments[0])
    //                .Concat(SelectExpression(app.Arguments[1]))
    //                .Append(new Instruction(jump, br1.Label))
    //                .Append(new Instruction(CilOp.Br, br2.Label));
    //        }
    //        else
    //        {
    //            return SelectExpression(cond)
    //                .Concat(SelectExpression(Boole.False))
    //                .Append(new Instruction(CilOp.BrEq, br2.Label))
    //                .Append(new Instruction(CilOp.Br, br1.Label));
    //        }
    //    }

    //    private static GoTo ExpectGoTo(ITail tail)
    //    {
    //        if (tail is GoTo output)
    //        {
    //            return output;
    //        }
    //        else
    //        {
    //            throw new Exception($"Expected tail in form of GoTo: {tail}");
    //        }
    //    }

    //    private static IEnumerable<Instruction> SelectExpression(ICpsExp exp)
    //    {
    //        return exp switch
    //        {
    //            Application app => SelectApplication(app.Operator, app.Arguments),
    //            VarBase v => [new Instruction(CilOp.Load, new TempVar(v))],
    //            IAtom atm => [new Instruction(CilOp.Load, atm)],
    //            _ => throw new Exception($"Can't select instructions for unknown expression type: {exp}")
    //        };
    //    }

    //    private static IEnumerable<Instruction> SelectApplication(PrimitiveOperator op, ICpsExp[] args)
    //    {
    //        IEnumerable<Instruction> opInstrs = SelectOperator(op);

    //        if (args.Length > 0)
    //        {
    //            IEnumerable<Instruction> argInstrs = SelectExpression(args[0]);

    //            for (int i = 1; i < args.Length; ++i)
    //            {
    //                if (args[i].Equals(args[i - 1]))
    //                {
    //                    argInstrs = argInstrs.Append(new Instruction(CilOp.Dupe));
    //                }
    //                else
    //                {
    //                    argInstrs = argInstrs.Concat(SelectExpression(args[i]));
    //                }
    //            }

    //            return argInstrs.Concat(opInstrs);
    //        }
    //        else
    //        {
    //            return opInstrs;
    //        }
    //    }

    //    private static IEnumerable<Instruction> SelectOperator(PrimitiveOperator op)
    //    {
    //        return op switch
    //        {
    //            PrimitiveOperator.Read => ConstructReadCall(),

    //            PrimitiveOperator.Add => [new Instruction(CilOp.Add)],
    //            PrimitiveOperator.Sub => [new Instruction(CilOp.Sub)],
    //            PrimitiveOperator.Neg => [new Instruction(CilOp.Neg)],

    //            PrimitiveOperator.Not => [new Instruction(CilOp.BitwiseNot)],

    //            _ => throw new Exception($"Can't select instruction(s) for unknown operator: {op}")
    //        };
    //    }

    //    private static IEnumerable<Instruction> ConstructReadCall()
    //    {
    //        yield return new Instruction(CilOp.Call, new Label("string [System.Console]System.Console::ReadLine()"));
    //        yield return new Instruction(CilOp.Call, new Label("int32 [System.Runtime]System.Int32::Parse(string)"));
    //    }
    //}
}
