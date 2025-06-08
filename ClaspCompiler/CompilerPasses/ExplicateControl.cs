using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCps;
using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ExplicateControl
    {
        public static Prog_Cps Execute(Prog_Sem program)
        {
            Dictionary<Label, ITail> blocks = [];
            int blockCounter = 10;

            ITail entry = ExplicateTail(program.Body, blocks, ref blockCounter);
            blocks.Add(Prog_Cps.StartLabel, entry);

            return new("()", blocks);
        }

        private static ITail ExplicateTail(ISemExp input, Dictionary<Label, ITail> blocks, ref int blockCounter)
        {
            switch (input)
            {
                case Let let:
                    ITail body = ExplicateTail(let.Body, blocks, ref blockCounter);
                    return ExplicateAssignment(let.Variable, let.Argument, body, blocks, ref blockCounter);

                case If branch:
                    ITail br1 = ExplicateTail(branch.Consequent, blocks, ref blockCounter);
                    ITail br2 = ExplicateTail(branch.Alternative, blocks, ref blockCounter);
                    return ExplicatePredicate(branch.Condition, br1, br2, blocks, ref blockCounter);

                case ISemExp exp:
                    return ExplicateReturning(exp);

                default:
                    throw new Exception($"Can't explicate control of unknown semantic form: {input}");
            }
        }

        private static ITail ExplicateAssignment(Var var, ISemExp bound, ITail tail, Dictionary<Label, ITail> blocks, ref int blockCounter)
        {
            switch (bound)
            {
                case Let let:
                    ITail body = ExplicateAssignment(var, let.Body, tail, blocks, ref blockCounter);
                    return ExplicateAssignment(let.Variable, let.Argument, body, blocks, ref blockCounter);

                case If branch:
                    ITail unbranch = CreateBlock(tail, blocks, ref blockCounter);
                    ITail br1 = ExplicateAssignment(var, branch.Consequent, unbranch, blocks, ref blockCounter);
                    ITail br2 = ExplicateAssignment(var, branch.Alternative, unbranch, blocks, ref blockCounter);
                    return ExplicatePredicate(branch.Condition, br1, br2, blocks, ref blockCounter);

                case ISemExp exp:
                    return new Sequence(new Assignment(var, TranslateExpression(exp)), tail);

                default:
                    throw new Exception($"Can't explicate assignment of unknown semantic form: {bound}");

            }
        }

        private static Conditional ExplicatePredicate(ISemExp cond, ITail branch1, ITail branch2, Dictionary<Label, ITail> blocks, ref int blockCounter)
        {
            //ICpsExp condition = TranslateBooleanCondition(cond);
            GoTo br1 = CreateBlock(branch1, blocks, ref blockCounter);
            GoTo br2 = CreateBlock(branch2, blocks, ref blockCounter);
            return new Conditional(TranslateExpression(cond), br1, br2);
        }

        private static Return ExplicateReturning(ISemExp value)
        {
            return new Return(TranslateExpression(value));
        }

        private static GoTo CreateBlock(ITail tail, Dictionary<Label, ITail> blocks, ref int blockCounter)
        {
            Label label = new Label($"block{blockCounter++}");
            blocks.Add(label, tail);
            return new GoTo(label);
        }



        //private static ICpsExp TranslateBooleanCondition(ISemExp exp)
        //{


        //    ISemExp calc = exp.Type == AtomicType.Bool
        //        ? exp
        //        : new PrimitiveApplication(PrimitiveOperator.Not, new PrimitiveApplication(PrimitiveOperator.Eq, Bool.False, exp));

        //    return TranslateExpression(calc);
        //}

        private static ICpsExp TranslateExpression(ISemExp value)
        {
            return value switch
            {
                ISemApp app => ExplicateApplication(app),
                IAtom atm => atm,
                Var v => v,
                _ => throw new Exception($"Can't translate non-calculable expression: {value}")
            };
        }

        private static ICpsExp ExplicateApplication(ISemApp app)
        {
            if (app is PrimitiveApplication primApp)
            {
                ICpsExp output = new Application(
                    primApp.Operator,
                    primApp.Arguments.Select(TranslateExpression).ToArray());

                return output;
            }

            throw new Exception($"Can't explicate control of unknown application form: {app}");
        }
    }
}
