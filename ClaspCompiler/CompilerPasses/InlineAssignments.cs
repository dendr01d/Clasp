using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCps;
using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeData;

namespace ClaspCompiler.CompilerPasses
{
    //internal static class InlineAssignments
    //{
    //    public static Prog_Cps Execute(Prog_Cps program)
    //    {
    //        Dictionary<Label, ITail> newLabeledTails = program.LabeledTails
    //            .ToDictionary(x => x.Key, x => InlineInTail(x.Value, new Dictionary<VarBase, ICpsExp>()));

    //        return new Prog_Cps(program.Info, newLabeledTails);
    //    }

    //    private static ITail InlineInTail(ITail tail, Dictionary<VarBase, ICpsExp> remap)
    //    {
    //        return tail switch
    //        {
    //            Sequence seq => InlineInSequence(seq, remap),
    //            Conditional br => InlineInConditional(br, remap),
    //            GoTo jmp => jmp,
    //            Return ret => ProcessReturn(ret, remap),
    //            _ => throw new Exception($"Can't perform inlining on unknown tail form: {tail}")
    //        };
    //    }

    //    private static Return ProcessReturn(Return ret, Dictionary<VarBase, ICpsExp> remap)
    //    {
    //        return new Return(ProcessExpression(ret.Value, remap));
    //    }

    //    private static ICpsExp ProcessExpression(ICpsExp exp, Dictionary<VarBase, ICpsExp> remap)
    //    {
    //        return exp switch
    //        {
    //            Application app => new Application(app.Operator, app.Arguments.Select(x => ProcessExpression(x, remap))),
    //            VarBase v => remap.TryGetValue(v, out ICpsExp? mapOut) ? mapOut : v,
    //            _ => exp
    //        };
    //    }

    //    private static ITail InlineInSequence(Sequence seq, Dictionary<VarBase, ICpsExp> remap)
    //    {
    //        if (seq.Statement is Assignment assgn)
    //        {
    //            return InlineAssignment(assgn, seq.Tail, remap);
    //        }
    //        else
    //        {
    //            return seq;
    //        }
    //    }

    //    private static ITail InlineInConditional(Conditional branch, Dictionary<VarBase, ICpsExp> remap)
    //    {
    //        // basically just check whether processing the condition allows us to prune a branch

    //        ICpsExp updatedCond = ProcessExpression(branch.Condition, remap);

    //        if (updatedCond is Boole b)
    //        {
    //            return b.Value
    //                ? branch.Consequent
    //                : branch.Alternative;
    //        }
    //        else
    //        {
    //            return new Conditional(updatedCond, InlineInTail(branch.Consequent, remap), InlineInTail(branch.Alternative, remap));
    //        }
    //    }

    //    private static ITail InlineAssignment(Assignment assgn, ITail tail, Dictionary<VarBase, ICpsExp> remap)
    //    {
    //        if (TryInlineAssignment(assgn.Variable, assgn.Value, tail, remap, out ITail? preResult))
    //        {
    //            return preResult;
    //        }
    //        else
    //        {
    //            ITail newTail = InlineInTail(tail, remap);

    //            if (TryInlineAssignment(assgn.Variable, assgn.Value, newTail, remap, out ITail? postResult))
    //            {
    //                return postResult;
    //            }
    //            else
    //            {
    //                return new Sequence(new Assignment(assgn.Variable, assgn.Value), newTail);
    //            }
    //        }
    //    }

    //    private static bool TryInlineAssignment(VarBase var, ICpsExp boundValue, ITail tail, Dictionary<VarBase, ICpsExp> remap,
    //        [NotNullWhen(true)] out ITail? result)
    //    {
    //        ICpsExp bound = ProcessExpression(boundValue, remap);

    //        if (!tail.FreeVariables.ContainsKey(var))
    //        {
    //            result = bound.IsIOBound()
    //                ? new Sequence(new SideEffect(bound), InlineInTail(tail, remap))
    //                : InlineInTail(tail, remap);
    //            return true;
    //        }
    //        else if (bound is ICpsArg)
    //        {
    //            result = InlineInTail(tail, new(remap) { { var, bound } });
    //            return true;
    //        }
    //        else if (!bound.IsIOBound())
    //        {
    //            int numUses = tail.FreeVariables.TryGetValue(var, out int count) ? count : 0;

    //            if (EstimateInlineCost(bound, numUses) < EstimateAssignedCost(bound, numUses))
    //            {
    //                result = InlineInTail(tail, new(remap) { { var, bound } });
    //                return true;
    //            }
    //        }

    //        result = null;
    //        return false;
    //    }

    //    #region Helpers

    //    private static int EstimateAssignedCost(ICpsExp exp, int uses)
    //    {
    //        return exp.EstimateComplexity() + 1 + (2 * uses);
    //    }

    //    private static int EstimateInlineCost(ICpsExp exp, int uses)
    //    {
    //        return exp.EstimateComplexity() * uses;
    //    }

    //    #endregion
    //}
}
