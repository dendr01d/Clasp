﻿using ClaspCompiler.IntermediateCLang;
using ClaspCompiler.CompilerPasses;
using ClaspCompiler.SchemeData;
using ClaspCompiler.IntermediateStackLang;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.Tokens;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateLocLang;

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
            "(let ([a 42]) (let ([b a]) b))",
            "(let ([v 1]) (let ([w 46]) (let ([x (+ v 7)]) (let ([y (+ 4 x)]) (let ([z (+ x w)])(+ z (- y)))))))"
        };

        private static void Main(string[] args)
        {
            foreach (string program in testPrograms)
            {
                Var.ResetGenerator();
                
                Console.WriteLine(new string('=', 65));
                Console.WriteLine();

                AnnounceProgram("Raw Input", program);

                TokenStream tokens = Tokenize.Execute("Test Program", program);
                //AnnounceProgram("Token Stream", tokens);

                ProgS1 stx = ParseSyntax.Execute(tokens);
                //AnnounceProgram("Syntax", stx.ToString());

                ProgR1 semProg = ParseSemantics.Execute(stx);
                //AnnounceProgram("Semantics", semProg);

                ProgR1 semProgUniqueVars = Uniquify.Execute(semProg);
                //AnnounceProgram("Unique Vars", semProgUniqueVars);

                ProgR1 semProgNoComplex = RemoveComplexOperants.Execute(semProgUniqueVars);
                //AnnounceProgram("Removed Complex Operants", semProgNoComplex);

                ProgC0 normProg = ExplicateControl.Execute(semProgNoComplex);
                //AnnounceProgram("A-Normal Form", normProg);

                ProgC0 normTyped = TypeCheckVars.Execute(normProg);
                //AnnounceProgram("Checked Types", normTyped);

                ProgLoc0 ilProg = SelectInstructions.Execute(normTyped);
                //AnnounceProgram("Pseudo-IL", ilProg);

                //ProgIl0 ilProgNoVars = AssignHomes.Execute(ilProg);
                //AnnounceProgram("Assigned Local Variable Homes", ilProgNoVars);

                ProgLoc0 ilProgLiveMems = UncoverLive.Execute(ilProg);
                //AnnounceProgram("Analyzed Liveness of Mems", ilProgLiveMems);

                ProgLoc0 ilProgWithInterference = BuildInterferenceGraph.Execute(ilProgLiveMems);
                //AnnounceProgram("Built Interference Graph", ilProgWithInterference);

                ProgLoc0 ilProgHomedVars = AllocateRegisters.Execute(ilProgWithInterference);
                //AnnounceProgram("Assigned Homes to Vars", ilProgHomedVars);

                ProgStack0 ilProgPatched = PatchInstructions.Execute(ilProgHomedVars);
                AnnounceProgram("Patched weird instructions", ilProgPatched);
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
