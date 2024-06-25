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

        public static void TestBlock(string text)
        {
            foreach(string line in text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] pieces = line.Split("=>", StringSplitOptions.TrimEntries);
                TestIO(pieces[1], pieces[0]);
            }
        }

        public static void TestFailure<T>(string input)
            where T : Exception
        {
            Assert.ThrowsException<T>(() => Clasp.Interpreter.Interpret(input));
        }
    }
}
