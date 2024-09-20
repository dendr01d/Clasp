using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    public static class Interpreter
    {
        public static string Interpret(params string[] inputs)
        {
            Environment env = GlobalEnvironment.LoadStandard();

            Expression result = Error.Instance;

            foreach(string input in inputs)
            {
                Expression parsed = Parser.ParseText(input).Last();
                result = Evaluator.Evaluate(parsed, env);
            }

            return result.ToSerialized();
        }
    }
}
