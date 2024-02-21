using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal static class Tester
    {
        public static void TestIO(string output, string input)
        {
            Assert.AreEqual(output, Clasp.Interpreter.Interpret(input));
        }

        public static void TestFailure<T>(string input)
            where T : Exception
        {
            Assert.ThrowsException<T>(() => Clasp.Interpreter.Interpret(input));
        }
    }
}
