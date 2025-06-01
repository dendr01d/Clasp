using ClaspCompiler.IntermediateCLang;
using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.IntermediateCLang.Abstract;
using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ExplicateControl
    {
        public static ProgC0 Execute(ProgR1 program)
        {
            ITail explicated = ExplicateTail(program.Body);

            return new(explicated);
        }

        private static ITail ExplicateTail(ISemanticExp exp)
        {
            return exp switch
            {
                Let l => ExplicateAssignment(l.Variable, l.Argument, ExplicateTail(l.Body)),
                _ => ExplicateReturning(exp)
            };
        }

        private static ITail ExplicateAssignment(Var var, ISemanticExp exp, ITail tail)
        {
            if (exp is Let l)
            {
                return ExplicateAssignment(l.Variable, l.Argument, ExplicateAssignment(var, l.Body, tail));
            }
            else
            {
                return new Sequence(
                    new Assignment(var, TranslateExpression(exp)),
                    tail);
            }
        }

        private static INormExp TranslateExpression(ISemanticExp exp)
        {
            if (exp is SchemeSemantics.Application app)
            {
                return new IntermediateCLang.Application(
                    app.Operator,
                    app.Arguments.Select(TranslateArgument).ToArray());
            }
            else if (exp is INormExp lit)
            {
                return lit;
            }
            else
            {
                throw new Exception($"Can't translate expression: {exp}");
            }
        }

        private static INormArg TranslateArgument(ISemanticExp exp)
        {
            return exp switch
            {
                IAtom atm => atm,
                Var v => v,
                _ => throw new Exception($"Can't translate argument: {exp}")
            };
        }

        private static ITail ExplicateReturning(ISemanticExp exp)
        {
            if (exp is IAtom atm)
            {
                return new Return(atm);
            }
            else
            {
                Var newVar = Var.Gen();

                return new Sequence(
                    new Assignment(newVar, TranslateExpression(exp)),
                    new Return(newVar));
            }
        }

    }
}
