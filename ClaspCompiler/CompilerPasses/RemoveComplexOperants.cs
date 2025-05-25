using ClaspCompiler.Common;
using ClaspCompiler.Semantics;

using BindingStack = System.Collections.Generic.Stack<System.Tuple<ClaspCompiler.Common.Var, ClaspCompiler.Semantics.ISemExp>>;

namespace ClaspCompiler.CompilerPasses
{
    internal static class RemoveComplexOperants
    {
        public static ProgR1 Execute(ProgR1 program)
        {
            return new ProgR1(program.Info, TransformNewScope(program.Body));
        }

        private static ISemExp TransformNewScope(ISemExp exp)
        {
            BindingStack bindings = new();

            ISemExp output = TransformComplexExpression(exp, bindings);

            while (bindings.TryPop(out var newBinding))
            {
                output = new Let(newBinding.Item1, newBinding.Item2, output);
            }

            return output;
        }

        private static ISemExp TransformComplexExpression(ISemExp exp, BindingStack bindings)
        {
            return exp switch
            {
                Let let => new Let(
                    let.Variable,
                    TransformComplexExpression(let.Argument, bindings),
                    TransformNewScope(let.Body)),
                Application app => new Application(
                    SimplifyExpression(app.Operator, bindings),
                    app.Arguments.Select(x => SimplifyExpression(x, bindings)).ToArray()),
                _ => exp
            };
        }

        private static ISemExp SimplifyExpression(ISemExp exp, BindingStack bindings)
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
                else if (let.Body is ILiteral lit)
                {
                    return lit;
                }

                //bindings.Push(new(let.Variable, let.Argument));
                //return SimplifyExpression(let.Body, bindings);
            }
            else
            if (exp is Application app)
            {
                Var newVar = Var.GenVar();
                bindings.Push(new(newVar, app));
                return newVar;
            }

            return exp;
        }
    }
}
