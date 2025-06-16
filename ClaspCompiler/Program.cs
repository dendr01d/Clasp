using System.ComponentModel;
using System.Text;

using ClaspCompiler.CompilerPasses;
using ClaspCompiler.IntermediateCil;
using ClaspCompiler.IntermediateCps;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.Tokens;

namespace ClaspCompiler
{
    internal class Program
    {
        private static readonly string[] _testPrograms =
        [
            //"(+ (read) (- (+ 5 3)))",
            //"(let ([x 32]) (+ (let ([x 10]) x) x))",
            //"(let ([x (let ([x 4]) (+ x 1))]) (+x 2))",
            //"(+ 52 (- 10))",
            //"(let ([a 42]) (let ([b a]) b))",
            //"(let ([y (let ([x 20]) (+ x (let ([x 22]) x)))]) y)",

            //"(let ([x (+ 12 20)]) (+ 10 x))",
            //"(let ([x (read)]) (let ([y (read)]) (+ x (- y))))",
            //"(let ([v 1]) (let ([w 46]) (let ([x (+ v 7)]) (let ([y (+ 4 x)]) (let ([z (+ x w)])(+ z (- y)))))))",

            //"(let ((x (read))) (let ((y (read))) (let ((z (+ x x))) (let ((w (- z))) (let ((a (+ z y))) (let ((b 5)) (let ((c y)) (+ x w))))))))",

            //"(let ((x (read))) (if (< x 0) (- x) (+ 10 x)))",

            //"(if (if (eq? (read) 1) (eq? (read) 0) (eq? (read) 2)) (+ 10 32) (+ 700 77))",

            //"(let ([x (read)]) (let ([y (read)]) (if (if (< x 1) (eq? x 0) (eq? x 2)) (+ y 2) (+ y 10))))"

            //"(vector-ref (vector-ref (vector (vector 42)) 0) 0)"

            "((lambda (x) (+ x 2)) 3)"
        ];

        private static void Main()
        {
            Console.WriteLine();

            //PrintAllCharacters();
            //return;

            int counter = 1;

            foreach (string program in _testPrograms)
            {
                Symbol.ResetInterment();

                Console.WriteLine(new string('=', 65));
                Console.WriteLine();

                AnnounceProgram("Raw Input", program);

                TokenStream tokens = Tokenize.Execute($"Test Program #{counter}", program);
                AnnounceProgram("Token Stream", tokens);

                Prog_Stx stxProg = ParseSyntax.Execute(tokens);
                AnnounceProgram("Scheme Syntax", stxProg);

                Prog_Stx stxPainted = PaintLexicalScopes.Execute(stxProg);
                Prog_Stx stxScoped = UniquifyByScope.Execute(stxPainted);
                AnnounceProgram("Scoped Identifiers", stxScoped);

                Prog_Sem semProg = ParseSemantics.Execute(stxScoped);
                AnnounceProgram("Scheme Semantics", semProg);

                //Prog_Sem semProgTypeChecked = TypeCheckSemantics.Execute(semProg);
                //AnnounceProgram("Type-Checked Semantics", semProgTypeChecked);

                //Prog_Sem semProgSimpleArgs = RemoveComplexOpera.Execute(semProgTypeChecked);
                //AnnounceProgram("Simplified Opera*", semProgSimpleArgs);

                ////Prog_Sem semProgSimpleMath = SimplifyMath.Execute(semProgSimpleArgs);
                ////AnnounceProgram("Simplified Math", semProgSimpleMath);

                //Prog_Cps cpsProg = ExplicateControl.Execute(semProgSimpleArgs);
                //AnnounceProgram("Explicated Control", cpsProg);

                //Prog_Cps cpsInlined = InlineAssignments.Execute(cpsProg);
                //AnnounceProgram("Inlined Redundant Assignments", cpsInlined);

                //////another math pass here

                //Prog_Cil cilProg = SelectInstructions.Execute(cpsInlined);
                //AnnounceProgram("Selected CIL Instructions", cilProg);

                //Prog_Cil cilProgWithLiveness = UncoverLive.Execute(cilProg);
                //Prog_Cil cilProgWithInterference = BuildInteferenceGraph.Execute(cilProgWithLiveness);
                //Prog_Cil cilProgWithLocalHomes = AllocateRegisters.Execute(cilProgWithInterference);
                //AnnounceProgram("Allocated Homes for Locals", cilProgWithLocalHomes);

                //Console.WriteLine();
                //AnnounceProgram("CPS-Translated Program", cpsInlined);
                //Console.WriteLine();

                //Console.WriteLine("*** Running Interpreter ***");
                //ICpsExp output = Interpreter.Interpret(cpsInlined, Prog_Cps.StartLabel);
                //Console.WriteLine("--> {0}", output);


                Console.WriteLine();

                counter++;
            }

            Console.WriteLine();
            Console.Write("Press any key to continue");
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

        private static void PrintAllCharacters()
        {
            Console.OutputEncoding = Encoding.UTF8;

            for (int i = 0; i < (1 << 14);)
            {
                Console.Write("{0,8}: ", i);
                Console.BackgroundColor = ConsoleColor.DarkCyan;
                Console.Write("0x{0:X5}:", i);
                Console.ResetColor();

                for (int j = 0; j < 8; ++j)
                {
                    Console.Write(' ');

                    for (int k = 0; k < 8; ++k)
                    {
                        char c = char.IsControl((char)i)
                            ? '?'
                            : Convert.ToChar(i);
                        Console.Write(c);
                        ++i;
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine("\n\nDone!");
            Console.ReadKey(true);
        }
    }
}
