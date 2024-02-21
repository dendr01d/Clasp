using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp;

namespace Tests
{
    [TestClass]
    public class SpecialForms
    {
        [TestClass]
        public class If
        {
            [TestMethod]
            public void ArgsZero() => Tester.TestFailure<ArgumentArityException>("(if)");
            [TestMethod]
            public void ArgsOne() => Tester.TestFailure<ArgumentArityException>("(if 1)");
            [TestMethod]
            public void ArgsTwo() => Tester.TestFailure<ArgumentArityException>("(if 1 2)");


            [TestMethod]
            public void CaseTrue() => Tester.TestIO("1", "(if 1 1 2)");

            [TestMethod]
            public void CaseFalse() => Tester.TestIO("2", "(if () 1 2)");

            [TestMethod]
            public void SubConditional() => Tester.TestIO("1", "(if (if 1 1 ()) 1 2)");

            [TestMethod]
            public void SubConsequent() => Tester.TestIO("5", "(if () 1 (+ 2 3))");

            [TestMethod]
            public void SelectiveEvaluation()
            {
                string test = "(begin (define var 0) (if () (set! var 1) (set! var 2)) var)";
                Tester.TestIO("2", test);
            }
        }

        [TestClass]
        public class Cond
        {
            [TestMethod]
            public void CaseSimpleFirst() => Tester.TestIO("1", "(cond (0 1) (2 3))");

            [TestMethod]
            public void CaseSimpleSecond() => Tester.TestIO("3", "(cond (() 1) (2 3))");

            [TestMethod]
            public void CaseComplexSecond() => Tester.TestIO("3", "(cond (() 1) (2 3) (4 5))");

            [TestMethod]
            public void CaseThird() => Tester.TestIO("5", "(cond (() 1) (() 3) (4 5))");

            [TestMethod]
            public void SubConditional() => Tester.TestIO("1", "(cond ((if 1 1 ()) 1) (2 3))");

            [TestMethod]
            public void SubConsequent() => Tester.TestIO("11", "(cond (0 (if () 10 11)) (2 3))");

            [TestMethod]
            public void SelectiveEvaluation()
            {
                string test = "(begin (define var 0) (cond (() (set! var 1)) ((set! var 2) var)))";
                Tester.TestIO("2", test);
            }

            [TestMethod]
            public void FalloutException()
            {
                Tester.TestFailure<ControlFalloutException>("(begin (define var 0) (cond (() ()) (() ())))");
            }
        }

        [TestClass]
        public class Quote
        {
            [TestMethod]
            public void SymbolMacro() => Tester.TestIO("a", "'a");
            [TestMethod]
            public void SymbolConcrete() => Tester.TestIO("a", "(quote a)");

            [TestMethod]
            public void EmptyMacro() => Tester.TestIO("()", "'()");
            [TestMethod]
            public void EmptyConcrete() => Tester.TestIO("()", "(quote ())");

            [TestMethod]
            public void ListMacro() => Tester.TestIO("(1 2 3)", "'(1 2 3)");
            [TestMethod]
            public void ListConcrete() => Tester.TestIO("(1 2 3)", "(quote (1 2 3))");

            [TestMethod]
            public void DottedMacro() => Tester.TestIO("(1 . 2)", "'(1 . 2)");
            [TestMethod]
            public void DottedConcrete() => Tester.TestIO("(1 . 2)", "(quote (1 . 2))");
        }

        [TestClass]
        public class Eq
        {
            [TestMethod]
            public void SymbolSame() => Tester.TestIO("#f", "(eq? 'a 'a)");
            [TestMethod]
            public void SymbolDifferent() => Tester.TestIO("#f", "(eq? 'a 'b)");
            [TestMethod]
            public void SymbolEq() => Tester.TestIO("#t", "((lambda (x) (eq? x x)) 'a)");


            [TestMethod]
            public void NumberSame() => Tester.TestIO("#f", "(eq? 5 5)");
            [TestMethod]
            public void NumberDifferent() => Tester.TestIO("#f", "(eq? 5 6)");
            [TestMethod]
            public void NumberEq() => Tester.TestIO("#t", "((lambda (x) (eq? x x)) 5)");


            [TestMethod]
            public void ProcedureSame() => Tester.TestIO("#t", "(eq? + +)");
            [TestMethod]
            public void ProcedureDifferent() => Tester.TestIO("#f", "(eq? + -)");
            [TestMethod]
            public void ProcedureEq() => Tester.TestIO("#t", "((lambda (x) (eq? x x)) +)");


            [TestMethod]
            public void EmptySame() => Tester.TestIO("#t", "(eq? () ())");
            [TestMethod]
            public void EmptyEq() => Tester.TestIO("#t", "((lambda (x) (eq? x x)) ())");


            [TestMethod]
            public void PairSame() => Tester.TestIO("#f", "(eq? (cons 'a 'b) (cons 'a 'b))");
            [TestMethod]
            public void PairDifferent() => Tester.TestIO("#f", "(eq? (cons 'a 'b) (cons 'c 'd))");
            [TestMethod]
            public void PairEq() => Tester.TestIO("#t", "((lambda (x) (eq? x x)) (cons 'a 'b))");
        }

        [TestClass]
        public class Cons
        {
            [TestMethod]
            public void CombineZero() => Tester.TestIO("()", "(cons '() '())");
            [TestMethod]
            public void CombineOne() => Tester.TestIO("(a)", "(cons 'a '())");
            [TestMethod]
            public void CombineTwo() => Tester.TestIO("(a . b)", "(cons 'a 'b)");
            [TestMethod]
            public void CombineThree() => Tester.TestIO("(a b . c)", "(cons 'a (cons 'b 'c))");
            [TestMethod]
            public void CombineTree() => Tester.TestIO("((a . b) c . d)", "(cons (cons 'a 'b) (cons 'c 'd))");
            [TestMethod]
            public void CombineOnEmpty() => Tester.TestIO("(() . a)", "(cons () 'a)");
            [TestMethod]
            public void ListOfTwo() => Tester.TestIO("(a b)", "(cons 'a (cons 'b '()))");
        }
    
    
    
    }
}
