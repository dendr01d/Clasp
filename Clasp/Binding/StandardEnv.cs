using Clasp.Binding.Environments;
using Clasp.Data;
using Clasp.Data.Terms;

namespace Clasp.Binding
{
    internal static class StandardEnv
    {
        public static SuperEnvironment CreateNew()
        {
            SuperEnvironment output = new SuperEnvironment();

            output.DefineCoreForm(Implicit.SpTop);
            output.DefineCoreForm(Implicit.SpVar);

            output.DefineCoreForm(Symbol.Quote);
            output.DefineCoreForm(Implicit.SpDatum);

            output.DefineCoreForm(Symbol.QuoteSyntax);

            output.DefineCoreForm(Symbol.Apply);
            output.DefineCoreForm(Implicit.SpApply);

            output.DefineCoreForm(Implicit.ParDef);
            output.DefineCoreForm(Symbol.Define);
            output.DefineCoreForm(Symbol.DefineSyntax);
            output.DefineCoreForm(Symbol.Set);

            output.DefineCoreForm(Symbol.Lambda);
            output.DefineCoreForm(Implicit.SpLambda);

            output.DefineCoreForm(Symbol.If);
            output.DefineCoreForm(Symbol.Begin);

            return output;
        }

    }
}
