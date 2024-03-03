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
            return Parser.Parse(input).CallEval(Environment.StdEnv()).ToString();
            //try
            //{
            //    return Parser.Parse(input).Evaluate(Environment.StdEnv()).ToString();
            //}
            //catch (Exception ex)
            //{
            //    return ex.Message;
            //}
        }


    }
}
