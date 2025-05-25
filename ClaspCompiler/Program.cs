using ClaspCompiler.ANormalForms;
using ClaspCompiler.CompilerPasses;
using ClaspCompiler.Data;
using ClaspCompiler.Semantics;
using ClaspCompiler.Syntax;
using ClaspCompiler.Tokens;

namespace ClaspCompiler
{
    internal class Program
    {
        private static readonly string[] testPrograms = new string[]
        {
            "(let ([x 32]) (+ (let ([x 10]) x) x))",
            "(let ([x (+ 12 20)]) (+ 10 x))",
            "(let ([x (read)]) (let ([y (read)]) (+ x (- y))))",
            "(let ([x (let ([x 4]) (+ x 1))]) (+ x 2))",
            "(+ 52 (- 10))",
            "(let ([a 42]) (let ([b a]) b))"
        };

        private static void Main(string[] args)
        {
            foreach (string program in testPrograms)
            {
                Symbol.ResetGenerator();

                Console.WriteLine(new string('=', 65));
                Console.WriteLine();

                AnnounceProgram("Raw Input", program);

                TokenStream tokens = Tokenize.Execute("Test Program", program);
                AnnounceProgram("Token Stream", tokens);

                ISyntax stx = ParseSyntax.Execute(tokens);
                AnnounceProgram("Syntax", stx.ToString());

                ProgR1 semProg = ParseSemantics.Execute(stx);
                AnnounceProgram("Semantics", semProg);

                ProgR1 semProgUniqueVars = Uniquify.Execute(semProg);
                AnnounceProgram("Unique Vars", semProgUniqueVars);

                ProgR1 semProgNoComplex = RemoveComplexOperants.Execute(semProgUniqueVars);
                AnnounceProgram("Removed Complex Operants", semProgNoComplex);

                ProgC0 normProg = ExplicateControl.Execute(semProgNoComplex);
                AnnounceProgram("A-Normal Form", normProg);
            }

            Console.WriteLine();
            Console.ReadKey(true);
        }

        private static void AnnounceProgram(string title, string text)
        {
            Console.WriteLine(":: {0} ::", title);
            Console.Write(' ');
            Console.WriteLine(text);
            Console.WriteLine('\n');
        }

        private static void AnnounceProgram(string title, IPrintable prin)
        {
            Console.WriteLine(":: {0} ::", title);
            Console.Write(' ');
            prin.Print(Console.Out, 1);
            Console.WriteLine("\n\n");
        }
    }
}
