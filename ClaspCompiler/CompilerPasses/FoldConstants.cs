using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class FoldConstants
    {
        public static ProgR1 Execute(ProgR1 program)
        {
            return new ProgR1(program.Info, FoldInExpression(program.Body));
        }

        private static ISemanticExp FoldInExpression(ISemanticExp exp)
        {
            return exp switch
            {
                Application app => FoldInApplication(app),
                ISpecialForm spec => FoldInSpecialForm(spec),
                _ => throw new Exception($"Can't fold in expression: {exp}")
            };
        }

        private static ISemanticExp FoldInApplication(Application app)
        {
            return app.Operator switch
            {
                PrimitiveOperator.ADD => FoldInAddition(app),
                PrimitiveOperator.NEG => FoldInNegation(app),
                _ => app
            };
        }

        private static ISemanticExp FoldInAddition(Application app)
        {
            IEnumerable<ISemanticExp> foldedArgs = app.Arguments.Select(FoldInExpression);

            IEnumerable<Integer> fixNumArgs = foldedArgs.OfType<Integer>();
            IEnumerable<ISemanticExp> otherArgs = foldedArgs.Except(fixNumArgs);

            Integer foldedNum = new(fixNumArgs.Sum(x => x.Value));

            if (!otherArgs.Any())
            {
                return foldedNum;
            }

            return new Application(PrimitiveOperator.ADD, SchemeType.Fixnum, [foldedNum, .. otherArgs]);
        }

        private static ISemanticExp FoldInNegation(Application app)
        {
            ISemanticExp arg = FoldInExpression(app.Arguments.First());

            if (arg is Integer i)
            {
                return new Integer(-1 * i.Value);
            }
            else
            {
                return new Application(PrimitiveOperator.NEG, SchemeType.Fixnum, arg);
            }
        }

        private static ISemanticExp FoldInSpecialForm(ISpecialForm spec)
        {
            return spec switch
            {
                Let l => FoldInLet(l),
                _ => throw new Exception($"Can't fold in unknown special form: {spec}")
            };
        }

        private static ISemanticExp FoldInLet(Let l)
        {
            return new Let(l.Variable,
                FoldInExpression(l.Argument),
                FoldInExpression(l.Body));
        }
    }
}
