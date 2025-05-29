using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSemantics.Abstract;

using BindingStack = System.Collections.Generic.Stack<System.Tuple<ClaspCompiler.CompilerData.Var, ClaspCompiler.SchemeSemantics.Abstract.ISemanticExp>>;

namespace ClaspCompiler.CompilerPasses
{
    internal static class RemoveComplexOperants
    {
        public static ProgR1 Execute(ProgR1 program)
        {
            return new ProgR1(program.Info, TransformNewScope(program.Body));
        }

        private static ISemanticExp TransformNewScope(ISemanticExp exp)
        {
            BindingStack bindings = new();

            ISemanticExp output = TransformComplexExpression(exp, bindings);

            while (bindings.TryPop(out var newBinding))
            {
                output = new Let(newBinding.Item1, newBinding.Item2, output);
            }

            return output;
        }

        private static ISemanticExp TransformComplexExpression(ISemanticExp exp, BindingStack bindings)
        {
            return exp switch
            {
                Let let => new Let(
                    let.Variable,
                    TransformComplexExpression(let.Argument, bindings),
                    TransformNewScope(let.Body)),
                Application app => new Application(
                    app.Operator,
                    app.Arguments.Select(x => SimplifyExpression(x, bindings)).ToArray()),
                _ => exp
            };
        }

        private static ISemanticExp SimplifyExpression(ISemanticExp exp, BindingStack bindings)
        {
            if (exp is Let let)
            {
                if (let.Body is Var var)
                {
                    if (let.Variable == var)
                    {
                        return let.Argument;
                    }
                }

                //bindings.Push(new(let.Variable, let.Argument));
                //return SimplifyExpression(let.Body, bindings);
            }
            else if (exp is Application app)
            {
                Var newVar = Var.Gen();
                bindings.Push(new(newVar, app));
                return newVar;
            }

            return exp;
        }
    }
}
