using ClaspCompiler.CompilerPasses;
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
            //"(+ . ((read . ()) . ((- . ((+ . (5 . (3 . ()))) . ())) . ()))))",

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

            //"`(lambda ((,x ',v)) ,body)",
            //"(list 'let (list (list x (list 'quote v))) body)",

            //"((lambda (x) (set-values! (y) (values (+ y x))) (define-values (y) (values x)) y) 5)",

            //@"
            //  (define var-add +)
            //  (set! var-add
            //        (lambda x
            //                (if (null? x)
            //                    0
            //                    (+ (car x)
            //                       (var-add (cdr x))))))
            //  (var-add 1 2 3 4)
            //  (var-add)
            //  (var-add 1 2)
            //  (var-add 1 2 3 4 5)",

            @"(define four (+ 2 2))
              four"
        ];

        private static void Main()
        {
            Console.WriteLine();

            int counter = 1;

            foreach (string program in _testPrograms)
            {
                Console.WriteLine(new string('=', 65));
                Console.WriteLine();

                AnnounceProgram("Raw Input", program);

                TokenStream tokens = Tokenize.Execute($"Test Program #{counter}", program);
                AnnounceProgram("Token Stream", tokens);

                Prog_Stx stxProg = ParseSyntax.Execute(tokens);
                AnnounceProgram("Scheme Syntax", stxProg);

                Prog_Stx stxExpanded = ExpandSyntax.Execute(stxProg);
                AnnounceProgram("Expanded Syntax", stxExpanded);

                Prog_Sem semProg = ParseSemantics.Execute(stxExpanded);
                AnnounceProgram("Scheme Semantics", semProg);

                Prog_Sem semConstrainedTypes = ConstrainSemanticTypes.Execute(semProg);
                AnnounceProgram("Scheme Semantics w/ Type Constraints", semConstrainedTypes);

                Prog_Sem semProgTypeChecked = ResolveSemanticTypes.Execute(semConstrainedTypes);
                AnnounceProgram("Scheme Semantics w/ Checked Types", semProgTypeChecked);

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

                //Prog_Cil cilProgPrunedJumps = RemoveJumps.Execute(cilProgWithLocalHomes);
                //AnnounceProgram("Removed Extraneous Jumps", cilProgPrunedJumps);

                ////Console.WriteLine();
                ////AnnounceProgram("CPS-Translated Program", cpsInlined);
                ////Console.WriteLine();

                ////Console.WriteLine("*** Running Interpreter ***");
                ////ICpsExp output = Interpreter.Interpret(cpsInlined, Prog_Cps.StartLabel);
                ////Console.WriteLine("--> {0}", output);


                Console.WriteLine();

                counter++;
            }

            Console.WriteLine();
            Console.Write("Press any key to continue");
            Console.ReadKey(true);
            System.Console.ReadKey(true);
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