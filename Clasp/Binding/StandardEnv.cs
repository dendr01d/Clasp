using Clasp.Binding.Environments;
using Clasp.Data.Terms;

namespace Clasp.Binding
{
    internal static class StandardEnv
    {
        public static SuperEnvironment CreateNew()
        {
            SuperEnvironment output = new SuperEnvironment();

            output.DefineInitial(Symbol.Define.Name, Symbol.Define); //remove at some point?
            output.DefineInitial(Symbol.Set.Name, Symbol.Set);

            output.DefineInitial(Symbol.Quote.Name, Symbol.Quote);
            output.DefineInitial(Symbol.Syntax.Name, Symbol.Syntax);

            output.DefineInitial(Symbol.Begin.Name, Symbol.Begin);
            output.DefineInitial(Symbol.If.Name, Symbol.If);
            output.DefineInitial(Symbol.Lambda.Name, Symbol.Lambda);

            return output;
        }

    }
}
