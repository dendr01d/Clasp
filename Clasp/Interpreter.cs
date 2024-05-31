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
            try
            {
                return Evaluator.Evaluate(Parser.Parse(input), GlobalEnvironment.Standard()).ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


    }
}
