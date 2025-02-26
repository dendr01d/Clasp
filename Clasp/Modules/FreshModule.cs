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
        public readonly Syntax FreshModuleForm;

        private FreshModule(string name, Syntax stx) : base(name)
        {
            FreshModuleForm = stx;
        }

        public static FreshModule Read(string name, string path)
        {
            IEnumerable<string> file = Piper.PipeInFileContents(path);
            IEnumerable<Token> tokens = Lexer.LexLines(path, file);
            Syntax stx = Reader.ReadTokens(tokens);

            return new FreshModule(name, stx);
        }

        public static FreshModule FromSyntax(Identifier id, Syntax body)
        {
            return new FreshModule(id.Name, body);
        }
    }
}
