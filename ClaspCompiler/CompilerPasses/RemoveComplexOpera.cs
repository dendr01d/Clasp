using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSemantics.Abstract;

using BindingStack = System.Collections.Generic.Stack<System.Tuple<ClaspCompiler.CompilerData.Var, ClaspCompiler.SchemeSemantics.Abstract.ISemExp>>;

namespace ClaspCompiler.CompilerPasses
{
    internal static class RemoveComplexOpera
    {
        public static Prog_Sem Execute(Prog_Sem program)
        {
            ISemExp newBody = RcoNewScope(program.Body);

            return new Prog_Sem(program.VariableTypes, newBody);
        }

        /// <summary>
        /// Process removal of complex opera* from the "top" level. Bindings created in the removal process
        /// cannot bubble up past this point.
        /// </summary>
        private static ISemExp RcoNewScope(ISemExp input)
        {
            BindingStack bindings = new BindingStack();

            ISemExp output = RcoExpression(input, bindings);

            while (bindings.TryPop(out var newBinding))
            {
                output = new Let(newBinding.Item1, RcoExpression(newBinding.Item2, bindings), output);
            }

            return output;
        }

        /// <summary>
        /// Process removal of complex opera* from an expression that is, itself, allowed to remain complex.
        /// Bindings created will bubble up to the nearest "top" level.
        /// </summary>
        private static ISemExp RcoExpression(ISemExp input, BindingStack bindings)
        {
            return input switch
            {
                Let let => new Let(let.Variable, RcoArg(let.Argument, bindings), RcoNewScope(let.Body)),
                If branch => new If(RcoArg(branch.Condition, bindings), RcoExpression(branch.Consequent, bindings), RcoExpression(branch.Alternative, bindings)),
                PrimitiveApplication pApp => new PrimitiveApplication(pApp.Operator, pApp.Arguments.Select(x => RcoArg(x, bindings))),
                _ => input
            };
        }

        /// <summary>
        /// Process removal of complex opera* from an expression being used as an argument to an application form.
        /// Bindings created will bubble up to the nearest "top" level.
        /// </summary>
        private static ISemExp RcoArg(ISemExp input, BindingStack bindings)
        {
            if (input is Let let)
            {
                // Let forms are complex. Relocate the binding to a point above where the corresponding body is evaluated.

                bindings.Push(new(let.Variable, let.Argument));
                return RcoArg(let.Body, bindings);
            }
            else if (input is If branch)
            {
                // conditional expressions are complex
                Var newVar = new Var(Symbol.GenSym().Name);

                ISemExp newBranch = RcoExpression(branch, bindings);
                bindings.Push(new(newVar, newBranch));
                return newVar;
            }
            else if (input is PrimitiveApplication pApp)
            {
                if (pApp.Operator.HasSideEffect())
                {
                    // IO-Bound applications are complex. Evaluate them and bind their result independently from
                    // the expression in which it's used.
                    // I THINK this preserves temporality of calls?

                    Var newVar = new Var(Symbol.GenSym().Name);
                    bindings.Push(new(newVar, pApp));
                    return newVar;
                }
                else
                {
                    // Other application forms aren't inherently complex.

                    return new PrimitiveApplication(pApp.Operator, pApp.Arguments.Select(x => RcoArg(x, bindings)));
                }
            }
            else
            {
                return input;
            }
        }
    }
}
