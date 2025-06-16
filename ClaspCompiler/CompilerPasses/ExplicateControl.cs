using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCps;
using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ExplicateControl
    {
        public static Prog_Cps Execute(Prog_Sem program)
        {
            Dictionary<Label, ITail> blocks = [];
            Counter idGen = new(10);

            ITail entry = ExplicateTail(program.Body, blocks, idGen).Value;
            blocks.Add(Prog_Cps.StartLabel, entry);

            return new("()", blocks);
        }

        private static Lazy<ITail> ExplicateTail(ISemExp input, Dictionary<Label, ITail> blocks, Counter idGen)
        {
            switch (input)
            {
                case Let let:
                    Lazy<ITail> body = ExplicateTail(let.Body, blocks, idGen);
                    return ExplicateAssignment(let.Variable, let.Argument, body, blocks, idGen);

                case If branch:
                    Lazy<ITail> br1 = CreateBlock(ExplicateTail(branch.Consequent, blocks, idGen), blocks, idGen);
                    Lazy<ITail> br2 = CreateBlock(ExplicateTail(branch.Alternative, blocks, idGen), blocks, idGen);
                    return ExplicatePredicate(branch.Condition, br1, br2, blocks, idGen);

                default:
                    return ExplicateReturning(input);
            }
        }

        private static Lazy<ITail> ExplicateAssignment(Var var, ISemExp bound, Lazy<ITail> tail, Dictionary<Label, ITail> blocks, Counter idGen)
        {
            switch (bound)
            {
                case Let let:
                    Lazy<ITail> body = ExplicateAssignment(var, let.Body, tail, blocks, idGen);
                    return ExplicateAssignment(let.Variable, let.Argument, body, blocks, idGen);

                case If branch:
                    Lazy<ITail> unbranch = CreateBlock(tail, blocks, idGen);
                    Lazy<ITail> br1 = CreateBlock(ExplicateAssignment(var, branch.Consequent, unbranch, blocks, idGen), blocks, idGen);
                    Lazy<ITail> br2 = CreateBlock(ExplicateAssignment(var, branch.Alternative, unbranch, blocks, idGen), blocks, idGen);
                    return ExplicatePredicate(branch.Condition, br1, br2, blocks, idGen);

                default:
                    return new(new Sequence(new Assignment(var, TranslateExpression(bound)), tail.Value));

            }
        }

        private static Lazy<ITail> ExplicatePredicate(ISemExp cond, Lazy<ITail> mkBr1, Lazy<ITail> mkBr2, Dictionary<Label, ITail> blocks, Counter idGen)
        {
            switch (cond)
            {
                case Let let:
                    return new(new Sequence(
                        new Assignment(let.Variable, TranslateExpression(let.Argument)),
                        ExplicatePredicate(let.Body, mkBr1, mkBr2, blocks, idGen).Value));

                case If branch:
                    Lazy<ITail> br1 = CreateBlock(ExplicatePredicate(branch.Consequent, mkBr1, mkBr2, blocks, idGen), blocks, idGen);
                    Lazy<ITail> br2 = CreateBlock(ExplicatePredicate(branch.Alternative, mkBr1, mkBr2, blocks, idGen), blocks, idGen);
                    return ExplicatePredicate(branch.Condition, br1, br2, blocks, idGen);

                case SchemeSemantics.SemApp pApp:
                    return pApp.Operator == PrimitiveOperator.Not
                        ? ExplicatePredicate(pApp.Arguments[0], mkBr2, mkBr1, blocks, idGen)
                        : new(new Conditional(TranslateExpression(cond), mkBr1.Value, mkBr2.Value));

                case Boole b:
                    return b.Value
                        ? mkBr1
                        : mkBr2;

                default:
                    ICpsExp exp = TranslateExpression(cond);
                    ICpsApp newCond = new IntermediateCps.Application(PrimitiveOperator.Eq, exp, Boole.False);
                    return new(new Conditional(newCond, mkBr2.Value, mkBr1.Value));
            }
        }

        private static Lazy<ITail> ExplicateReturning(ISemExp value)
        {
            return new(new Return(TranslateExpression(value)));
        }

        private static Lazy<ITail> CreateBlock(Lazy<ITail> tail, Dictionary<Label, ITail> blocks, Counter idGen)
        {
            return new(() =>
            {
                Label label = new Label($"block{idGen.GetValue()}");
                blocks.Add(label, tail.Value);
                return new GoTo(label);
            });
        }

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
            if (app is SchemeSemantics.SemApp primApp)
            {
                ICpsExp output = new IntermediateCps.Application(
                    primApp.Operator,
                    primApp.Arguments.Select(TranslateExpression).ToArray());

                return output;
            }

            throw new Exception($"Can't explicate control of unknown application form: {app}");
        }
    }
}
