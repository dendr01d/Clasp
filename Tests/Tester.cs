using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal static class Tester
    {
        //public static void TestIO(string output, string input)
        //{
        //    Assert.AreEqual(output, Clasp.Interpreter.Interpret(input));
        //}

        //public static void TestBlock(TestBattery battery)
        //{
        //    int i = 0;

        //    foreach(var pair in battery)
        //    {
        //        try
        //        {
        //            TestIO(pair.Item1, pair.Item2);
        //        }
        //        catch (Exception ex)
        //        {
        //            string msg = string.Format("Exception in battery test #{0}:\n {1} --> {2}\n",
        //                i,
        //                pair.Item2,
        //                pair.Item1);
        //            throw new Exception(msg, ex);
        //        }

        //        ++i;
        //    }
        //}

        //public static void TestFailure<T>(string input)
        //    where T : Exception
        //{
        //    Assert.ThrowsException<T>(() => Clasp.Interpreter.Interpret(input));
        //}

        //public static void TestSequentialIO(string output, params string[] steps)
        //{
        //    Assert.AreEqual(output, Clasp.Interpreter.Interpret(steps));
        //}

        //public static void TestSequentialFailure<T>(params string[] steps)
        //    where T : Exception
        //{
        //    Assert.ThrowsException<T>(() => Clasp.Interpreter.Interpret(steps));
        //}
    }

    internal class TestBattery : IEnumerable<Tuple<string, string>>
    {
        private List<Tuple<string, string>> _battery;

        public TestBattery()
        {
            _battery = new List<Tuple<string, string>>();
        }

        public IEnumerator<Tuple<string, string>> GetEnumerator() => _battery.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _battery.GetEnumerator();

        public void Add(string output, string input)
        {
            _battery.Add(new Tuple<string, string>(output, input));
        }
    }
}
