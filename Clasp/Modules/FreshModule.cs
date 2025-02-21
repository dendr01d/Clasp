using System.Collections.Generic;

using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Exceptions;
using Clasp.Process;

namespace Clasp.Modules
{
    /// <summary>
    /// The syntax of a freshly-imported module form, without any processing work done.
    /// </summary>
    internal sealed class FreshModule : Module
    {
        public readonly Syntax FreshSyntax;

        private FreshModule(string name, Syntax stx) : base(name)
        {
            FreshSyntax = stx;
        }

        public static FreshModule LoadFromFile(string path)
        {
            IEnumerable<string> file = Piper.PipeInFileContents(path);
            IEnumerable<Token> tokens = Lexer.LexLines(path, file);
            Syntax stx = Reader.ReadTokens(tokens);

            if (stx is not SyntaxList stl
                || stl.Expose() is not Cons cns
                || cns.Cdr is not Cons cns2
                || cns.Car is not Identifier id)
            {
                throw new ReaderException.InvalidModuleForm(stx);
            }

            return new FreshModule(id.Name, stx);
        }
    }
}
