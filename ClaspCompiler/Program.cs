using System.Text;

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
            Console.OutputEncoding = Encoding.UTF8;

            //TestSpecialCharacters();
            //Console.ReadKey(true);

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

                //Prog_Sem semProgTypeChecked = ResolveSemanticTypes.Execute(semConstrainedTypes);
                //AnnounceProgram("Scheme Semantics w/ Checked Types", semProgTypeChecked);

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

        private static void TestSpecialCharacters()
        {
            //Console.WriteLine(" Punctuation: {0}", "¡ ‼ ‽ … ¶ ");
            //Console.WriteLine(" Greek Lower: {0}", "α β γ δ ε ζ η θ ι κ λ μ ν ξ ο π ρ σ τ υ φ χ ψ ω");
            //Console.WriteLine(" Greek Upper: {0}", "Α Β Γ Δ Ε Ζ Η Θ Ι Κ Λ Μ Ν Ξ Ο Π Ρ Σ Τ Υ Φ Χ Ψ Ω");
            //Console.WriteLine("Super-Script: {0}", "⁰ ¹ ² ³ ⁴ ⁵ ⁶ ⁷ ⁸ ⁹ ");
            //Console.WriteLine("  Sub-Script: {0}", "₀ ₁ ₂ ₃ ₄ ₅ ₆ ₇ ₈ ₉ ");
            //Console.WriteLine(" Prime Marks: {0}", "′ ″ ‴ ");
            //Console.WriteLine("  Mult & Div: {0}", "× ÷ " ) ; 
            //Console.WriteLine(" Quantifiers: {0}", "Ɐ Ǝ ∄" ) ;
            //Console.WriteLine("   Logic Ops: {0}", "∧ ∨ ⊕" ) ;
            //Console.WriteLine("   Misc Math: {0}", "∂ ∆ ∇ ∑ ∏ ∐ √ ∞ ∫ ");
            //Console.WriteLine("   Type Info: {0}", "⊤ ⊥ ⊣ ⊢ ⊳ ⊲");
            //Console.WriteLine("    Eq & Neg: {0}", "≠ ≡ ≢ ≈ ¬ ");
            //Console.WriteLine("     Set Ops: {0}", "⋃ ⋂ ⊃ ⊂ ∅ ∈ ∋");
            //Console.WriteLine("    Arrows 1: {0}", "← ↑ → ↓ ↔ ↕");
            //Console.WriteLine("    Arrows 2: {0}", "⇐ ⇑ ⇒ ⇓ ⇔ ⇕");
            //Console.WriteLine("Round Things: {0}", "• ✶ ° ∘");
            //Console.WriteLine("     Symbols: {0}", "♀ ♂ ♠ ♣ ♥ ♦");

            static void PrintBlock(int firstRowBegin, int lastRowBegin)
            {
                for (int i = firstRowBegin; i < lastRowBegin + 0x10; i += 0x10)
                {
                    for (int j = 0; j < 0x10; ++j)
                    {
                        Console.Write(' ');
                        Console.Write((char)(i + j));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            Console.WriteLine("Basic Latin");
            PrintBlock(0x0020, 0x0070);

            Console.WriteLine("Latin-1 Supp:");
            PrintBlock(0x00A0, 0x00F0);

            Console.WriteLine("Greek:");
            PrintBlock(0x0390, 0x03D0);

            Console.WriteLine("General Punctuation:");
            PrintBlock(0x2010, 0x2050);

            Console.WriteLine("Superscript & Subscript:");
            Console.WriteLine("  (\u00B9 \u00B2 \u00B3)");
            PrintBlock(0x2070, 0x2090);

            Console.WriteLine("Letter-Like Symbols:");
            PrintBlock(0x2100, 0x2140);

            Console.WriteLine("Arrows:");
            PrintBlock(0x2190, 0x21F0);

            Console.WriteLine("Math:");
            PrintBlock(0x2200, 0x22F0);

            Console.WriteLine("Geometric Shapes:");
            PrintBlock(0x25A0, 0x25F0);

            Console.WriteLine("Misc Symbols:");
            PrintBlock(0x2600, 0x26F0);

            Console.WriteLine("Dingbats:");
            PrintBlock(0x2700, 0x27B0);

            Console.WriteLine("Misc Math Symbols A:");
            PrintBlock(0x27C0, 0x27E0);

            Console.WriteLine("Misc Math Symbols B:");
            PrintBlock(0x2980, 0x29F0);

            Console.WriteLine("Supp Math Ops:");
            PrintBlock(0x2A00, 0x2AF0);
        }
    }
}