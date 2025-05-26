using ClaspCompiler.ANormalForms;
using ClaspCompiler.Common;
using ClaspCompiler.Semantics;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ExplicateControl
    {
        public static ProgC0 Execute(ProgR1 program)
        {
            ITail explicated = ExplicateTail(program.Body);

            return new(explicated);
        }

        private static ITail ExplicateTail(ISemExp exp)
        {
            return exp switch
            {
                Let l => ExplicateAssignment(l.Variable, l.Argument, ExplicateTail(l.Body)),
                _ => ExplicateReturning(exp)
            };
        }

        private static ITail ExplicateAssignment(Var var, ISemExp exp, ITail tail)
        {
            if (exp is Let l)
            {
                return ExplicateAssignment(l.Variable, l.Argument, ExplicateAssignment(var, l.Body, tail));
            }
            else
            {
                return new Sequence(
                    new Assign(var, TranslateExpression(exp)),
                    tail);
            }
        }

        private static INormExp TranslateExpression(ISemExp exp)
        {
            if (exp is IApplication<ISemExp> app)
            {
                return new ANormalForms.Application(
                    TranslateExpression(app.Operator),
                    app.Arguments.Select(TranslateExpression));
            }
            else if (exp is IAtom lit)
            {
                return lit;
            }
            else
            {
                throw new Exception($"Can't translate expression: {exp}");
            }
        }

        private static ITail ExplicateReturning(ISemExp exp)
        {
            if (exp is Semantics.Application)
            {
                Var newVar = Var.GenVar();

                return new Sequence(
                    new Assign(newVar, TranslateExpression(exp)),
                    new Return(newVar));
            }
            else
            {
                return new Return(TranslateExpression(exp));
            }
        }

    }
}
