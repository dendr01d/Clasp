using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    /// <summary>
    /// A function for computing the result of an Expression Evaluation, possibly only partially
    /// </summary>
    /// <param name="expr">The expression to be evaluated</param>
    /// <param name="env">The environment in which to evaluate the expression</param>
    /// <returns>A <see cref="Recurrence"/> containing either the final expression or the next step of its computation</returns>
    internal delegate Recurrence Continuation(Expression expr, Environment env);

    /// <summary>
    /// Represents the ongoing computation of an Expression Evaluation
    /// </summary>
    /// <param name="Result">The current state of the computation</param>
    /// <param name="Env">The environment in which <see cref="result"/> needs to be processed</param>
    /// <param name="Cont">The next function by which <see cref="result"/> needs to be processed</param>
    internal struct Recurrence(Expression expr, Environment? env, Continuation? cont)
    {
        public readonly Expression Result = expr;
        public readonly Environment? NextEnv = env;
        public readonly Continuation? NextFunc = cont;
    }

}
