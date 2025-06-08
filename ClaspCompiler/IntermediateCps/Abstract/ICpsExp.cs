using ClaspCompiler.CompilerData;

namespace ClaspCompiler.IntermediateCps.Abstract
{
    internal interface ICpsExp : IPrintable { }

    internal static class ICpsExpExtensions
    {
        public static Dictionary<Var, int> CountFreeVariables(this ICpsExp exp)
        {
            Dictionary<Var, int> output = new();

            foreach (Var v in EnumerateFreeVariables(exp))
            {
                if (!output.ContainsKey(v))
                {
                    output[v] = 0;
                }
                output[v]++;
            }

            return output;

            //return EnumerateFreeVariables(exp).ToArray();
        }

        private static IEnumerable<Var> EnumerateFreeVariables(ICpsExp exp)
        {
            Stack<ICpsExp> stack = new Stack<ICpsExp>();
            stack.Push(exp);

            while (stack.Count > 0)
            {
                ICpsExp next = stack.Pop();

                if (next is ICpsApp app)
                {
                    foreach (ICpsExp arg in app.Arguments) stack.Push(arg);
                }
                else if (next is Var var)
                {
                    yield return var;
                }
            }
        }

        public static int EstimateComplexity(this ICpsExp exp)
        {
            return exp switch
            {
                Application app => 1 + app.Arguments.Sum(EstimateComplexity),
                _ => 1
            };
        }

        public static bool IsIOBound(this ICpsExp exp)
        {
            return exp is ICpsApp app && app.IsIOBound();
        }
    }
}
