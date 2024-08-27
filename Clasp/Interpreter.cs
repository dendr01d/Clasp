using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    public static class Interpreter
    {
        public static string Interpret(string input)
        {
            Environment env = GlobalEnvironment.LoadStandard();

            string output = string.Empty;

            foreach (Expression expr in Parser.ParseText(input))
            {
                Expression result = Evaluator.Evaluate(expr, env);
                output = result.ToString();
            }

            return output;
        }
    }
}
