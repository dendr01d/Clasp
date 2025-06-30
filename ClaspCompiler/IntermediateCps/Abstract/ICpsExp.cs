using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.IntermediateCps.Abstract
{
    internal interface ICpsExp : IPrintable, IEquatable<ICpsExp> { }

    //internal static class ICpsExpExtensions
    //{
    //    public static Dictionary<VarBase, int> CountFreeVariables(this ICpsExp exp)
    //    {
    //        Dictionary<VarBase, int> output = new();

    //        foreach (VarBase v in EnumerateFreeVariables(exp))
    //        {
    //            if (output.TryGetValue(v, out int value))
    //            {
    //                output[v] = value + 1;
    //            }
    //            else
    //            {
    //                output[v] = 1;
    //            }
    //        }

    //        return output;

    //        //return EnumerateFreeVariables(exp).ToArray();
    //    }

    //    private static IEnumerable<VarBase> EnumerateFreeVariables(ICpsExp exp)
    //    {
    //        Stack<ICpsExp> stack = new Stack<ICpsExp>();
    //        stack.Push(exp);

    //        while (stack.Count > 0)
    //        {
    //            ICpsExp next = stack.Pop();

    //            if (next is ICpsApp app)
    //            {
    //                foreach (ICpsExp arg in app.Arguments) stack.Push(arg);
    //            }
    //            else if (next is VarBase var)
    //            {
    //                yield return var;
    //            }
    //        }
    //    }

    //    public static int EstimateComplexity(this ICpsExp exp)
    //    {
    //        return exp switch
    //        {
    //            Application app => 1 + app.Arguments.Sum(EstimateComplexity),
    //            _ => 1
    //        };
    //    }

    //    public static bool IsIOBound(this ICpsExp exp)
    //    {
    //        return exp is Application app
    //            && (app.Operator.HasSideEffect() || app.Arguments.Any(IsIOBound));
    //    }
    //}
}
