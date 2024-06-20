using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp;

namespace Tests
{
    [TestClass]
    public class StandardProcedures
    {
        [TestClass]
        public class AtomicEvaluation
        {
            [TestMethod]
            public void BooleanEval() => Tester.TestBlock(@"
#t                          =>  #t
#f                          =>  #f
'#f                         =>  #f");
        }

        [TestClass]
        public class LogicalEvaluation
        {
            [TestMethod]
            public void LogicalNot() => Tester.TestBlock(@"
(not #t)                    =>  #f
(not 3)                     =>  #f
(not (list 3))              =>  #f
(not #f)                    =>  #t
(not '())                   =>  #f
(not (list))                =>  #f
(not 'nil)                  =>  #f");


        }

        [TestClass]
        public class PredicateEvaluation
        {
            [TestMethod]
            public void PredBoolean() => Tester.TestBlock(@"
(boolean? #f)               =>  #t
(boolean? 0)                =>  #f
(boolean? '())              =>  #f");
        }

        [TestClass]
        public class EquivalenceChecks
        {
        }
    }

    //[TestClass]
    //public class SpecialForms
    //{
    //    [TestClass]
    //    public class If
    //    {
    //        [TestMethod]
    //        public void CaseTrue() => Tester.TestIO("1", "(if 1 1 2)");

    //        [TestMethod]
    //        public void CaseFalse() => Tester.TestIO("2", "(if () 1 2)");

    //        [TestMethod]
    //        public void SubConditional() => Tester.TestIO("1", "(if (if 1 1 ()) 1 2)");

    //        [TestMethod]
    //        public void SubConsequent() => Tester.TestIO("5", "(if () 1 (+ 2 3))");

    //        [TestMethod]
    //        public void SelectiveEvaluation()
    //        {
    //            string test = "(begin (define var 0) (if () (set! var 1) (set! var 2)) var)";
    //            Tester.TestIO("2", test);
    //        }
    //    }

    //    [TestClass]
    //    public class Cond
    //    {
    //        [TestMethod]
    //        public void CaseSimpleFirst() => Tester.TestIO("1", "(cond (0 1) (2 3))");

    //        [TestMethod]
    //        public void CaseSimpleSecond() => Tester.TestIO("3", "(cond (() 1) (2 3))");

    //        [TestMethod]
    //        public void CaseComplexSecond() => Tester.TestIO("3", "(cond (() 1) (2 3) (4 5))");

    //        [TestMethod]
    //        public void CaseThird() => Tester.TestIO("5", "(cond (() 1) (() 3) (4 5))");

    //        [TestMethod]
    //        public void SubConditionalTrue() => Tester.TestIO("1", "(cond ((if 1 1 ()) 1) (2 3))");

    //        [TestMethod]
    //        public void SubConditionalFalse() => Tester.TestIO("3", "(cond ((if 1 () 1) 1) (2 3))");

    //        [TestMethod]
    //        public void SubConsequent() => Tester.TestIO("11", "(cond (0 (if () 10 11)) (2 3))");

    //        [TestMethod]
    //        public void SelectiveEvaluation()
    //        {
    //            string test = "(begin (define var 0) (cond (() (set! var 1)) ((define new-var var) new-var)))";
    //            Tester.TestIO("0", test);
    //        }
    //    }

    //    [TestClass]
    //    public class Quote
    //    {
    //        [TestMethod]
    //        public void QuoteSymbol() => Tester.TestIO("a", "(quote a)");
    //        [TestMethod]
    //        public void QuoteProcedure() => Tester.TestIO("(+ 1 2)", "(quote (+ 1 2))");

    //        [TestMethod]
    //        public void QuoteMarkSymbol() => Tester.TestIO("a", "'a");
    //        [TestMethod]
    //        public void QuoteMarkEmpty() => Tester.TestIO("()", "'()");
    //        [TestMethod]
    //        public void QuoteMarkProcedure() => Tester.TestIO("(+ 1 2)", "'(+ 1 2");
    //        [TestMethod]
    //        public void QuoteMarkQuoteSymbol() => Tester.TestIO("(quote a)", "'(quote a)");
    //        [TestMethod]
    //        public void QuoteMarkQuoteMarkSymbol() => Tester.TestIO("(quote a)", "''a");

    //        [TestMethod]
    //        public void QuoteSelfString() => Tester.TestIO("\"abc\"", "'\"abc\"");
    //        [TestMethod]
    //        public void QuoteSelfNumber() => Tester.TestIO("31415", "'31415");
    //        [TestMethod]
    //        public void QuoteSelfBoolean() => Tester.TestIO("#t", "'#t");
    //    }

    //    [TestClass]
    //    public class Cons
    //    {
    //        [TestMethod]
    //        public void CombineZero() => Tester.TestIO("()", "(cons '() '())");
    //        [TestMethod]
    //        public void CombineOne() => Tester.TestIO("(a)", "(cons 'a '())");
    //        [TestMethod]
    //        public void CombineTwo() => Tester.TestIO("(a . b)", "(cons 'a 'b)");
    //        [TestMethod]
    //        public void CombineThree() => Tester.TestIO("(a b . c)", "(cons 'a (cons 'b 'c))");
    //        [TestMethod]
    //        public void CombineTree() => Tester.TestIO("((a . b) c . d)", "(cons (cons 'a 'b) (cons 'c 'd))");
    //        [TestMethod]
    //        public void CombineOnEmpty() => Tester.TestIO("(() . a)", "(cons () 'a)");
    //        [TestMethod]
    //        public void ListOfTwo() => Tester.TestIO("(a b)", "(cons 'a (cons 'b '()))");
    //    }
    
    
    
    //}
}
