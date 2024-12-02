//using Clasp;

//namespace Tests
//{
//    [TestClass]
//    public class StandardProcedures
//    {
//        [TestClass]
//        public class LiteralEvaluation
//        {
//            [TestMethod]
//            public void EvalBoolean() => Tester.TestBlock(new()
//            {
//                { "#t", "#t" },
//                { "#t", "'#t" },
//                { "#f", "#f" },
//                { "#f", "'#f" }
//            });

//            [TestMethod]
//            public void EvalCharacter() => Tester.TestBlock(new()
//            {
//                { @"#\a", @"#\a" },
//                { @"#\space", @"#\space" },
//                { @"#\newline", @"#\newline" },
//                { @"#\tab", @"#\tab" },
//            });

//            [TestMethod]
//            public void EvalCharString() => Tester.TestBlock(new()
//            {
//                { "\"Hello world!\"", "\"Hello world!\"" }
//            });
//        }

//        [TestClass]
//        public class NumberEvaluation
//        {
//            [TestMethod]
//            public void EvalSimpleNum() => Tester.TestBlock(new()
//            {
//                { "0", "0" },
//                { "1", "1" },
//                { "3.14", "3.14" },
//                { "5555", "5555" },
//            });
//        }

//        [TestClass]
//        public class PredicateApplication
//        {
//            [TestMethod]
//            public void PredicateAtom() => Tester.TestBlock(new()
//            {
//                { "#t", "(atom? 1)" },
//                { "#t", "(atom? 'a)" },
//                { "#t", "(atom? '())" },
//                { "#f", "(atom? '(0 . 1))" },
//                { "#f", "(atom? '(a . b))" },
//                { "#f", "(atom? '(1 . '()))" },
//            });

//            [TestMethod]
//            public void PredicatePair() => Tester.TestBlock(new()
//            {
//                { "#f", "(pair? 1)" },
//                { "#f", "(pair? 'a)" },
//                { "#f", "(pair? '())" },
//                { "#t", "(pair? '(0 . 1))" },
//                { "#t", "(pair? '(a . b))" },
//                { "#t", "(pair? '(1 . '()))" },
//            });

//            [TestMethod]
//            public void PredicateList() => Tester.TestBlock(new()
//            {
//                { "#f", "(list? 1)" },
//                { "#f", "(list? 'a)" },
//                { "#t", "(list? '())" },
//                { "#f", "(list? '(0 . 1))" },
//                { "#f", "(list? '(a . b))" },
//                { "#t", "(list? '(1 . '()))" },
//            });

//            [TestMethod]
//            public void PredicateNull() => Tester.TestBlock(new()
//            {
//                { "#t", "(null? '())" },
//                { "#f", "(null? 1)" },
//                { "#f", "(null? 'a)" },
//                { "#f", "(null? '('() . '()))" },
//                { "#f", "(null? '(1 . '()))" },
//            });

//            [TestMethod]
//            public void PredicateSymbol() => Tester.TestBlock(new()
//            {
//                { "#t", "(symbol? 'a)" },
//                { "#f", "(symbol? 1)" },
//                { "#f", "(symbol? '())" },
//                { "#f", "(symbol? '(a . b))" },
//            });

//            [TestMethod]
//            public void PredicateProcedure() => Tester.TestBlock(new()
//            {
//                { "#f", "(procedure? 'a)" },
//                { "#f", "(procedure? 1)" },
//                { "#f", "(procedure? '())" },
//                { "#t", "(procedure? +)" },
//                { "#f", "(procedure? '+)" },
//                { "#t", "(procedure? car)" },
//                { "#f", "(procedure? 'car)" },
//                { "#t", "(procedure? (lambda (x) x))" }
//            });

//            [TestMethod]
//            public void PredicateNumber() => Tester.TestBlock(new()
//            {
//                { "#f", "(number? 'a)" },
//                { "#t", "(number? 1)" },
//                { "#f", "(number? '())" },
//                { "#f", "(number? '(1 . 2))" },
//                { "#t", "(number? 0)" },
//                { "#t", "(number? 3.14)" },
//            });

//            [TestMethod]
//            public void PredicateBoolean() => Tester.TestBlock(new()
//            {
//                { "#f", "(boolean? 'a)" },
//                { "#f", "(boolean? 1)" },
//                { "#f", "(boolean? '())" },
//                { "#t", "(boolean? #t)" },
//                { "#t", "(boolean? #f)" },
//                { "#t", "(boolean? '#t)" },
//                { "#t", "(boolean? '#f)" },
//                { "#f", "(boolean? '(#t . #f))" }
//            });
//        }

//        [TestClass]
//        public class LogicalConnectives
//        {
//            [TestMethod]
//            public void LogicalNot() => Tester.TestBlock(new()
//            {
//                { "#t", "(not #f)" },
//                { "#f", "(not #t)" },
//                { "#f", "(not 1)" },
//                { "#f", "(not 'a)" },
//                { "#f", "(not '())" },
//            });

//            [TestMethod]
//            public void LogicalAnd() => Tester.TestBlock(new()
//            {
//                { "#t", "(and)" },
//                { "#t", "(and #t)" },
//                { "#t", "(and #t #t)" },
//                { "#f", "(and #f)" },
//                { "#f", "(and #f #f)" },
//                { "#f", "(and #f #t)" },
//                { "#f", "(and #t #f)" }
//            });

//            [TestMethod]
//            public void LogicalOr() => Tester.TestBlock(new()
//            {
//                { "#f", "(or)" },
//                { "#t", "(or #t)" },
//                { "#t", "(or #t #t)" },
//                { "#f", "(or #f)" },
//                { "#f", "(or #f #f)" },
//                { "#t", "(or #f #t)" },
//                { "#t", "(or #t #f)" }
//            });
//        }

//        [TestClass]
//        public class PrimitiveListOperations
//        {
//            [TestMethod]
//            public void EvalCons() => Tester.TestBlock(new()
//            {
//                { "(a)", "(cons 'a '())" },
//                { "((a) b c d)", "(cons '(a) '(b c d))" },
//                { "(a b c)", "(cons \"a\" '(b c))" },
//                { "(a . 3)", "(cons 'a 3)" },
//                { "((a b) . c)", "(cons '(a b) 'c)" }
//            });

//            [TestMethod]
//            public void EvalCar() => Tester.TestBlock(new()
//            {
//                { "a", "(car '(a b c))" },
//                { "(a)", "(car '((a) b c d)) " },
//                { "1", "(car '(1 . 2)) " },
//            });

//            [TestMethod]
//            public void EvalCdr() => Tester.TestBlock(new()
//            {
//                { "(b c d)", "(cdr '((a) b c d))  " },
//                { "2", "(cdr '(1 . 2))" }
//            });

//            //[TestMethod]
//            //public void EvalCarNil() => Tester.TestFailure<ExpectedTypeException<Pair>>("(car '())");

//            //[TestMethod]
//            //public void EvalCdrNil() => Tester.TestFailure<ExpectedTypeException<Pair>>("(cdr '())");

//        }

//        [TestClass]
//        public class EquivalencePredicates
//        {
//            [TestMethod]
//            public void EvalEq() => Tester.TestBlock(new()
//            {
//                { "#t", "(eq? '() '())" },
//                { "#t", "(eq? 'a 'a)" },
//                { "#t", "(eq? + +)" },
//                { "#t", @"((lambda (x) (eq? x x)) #\a)" },
//                { "#f", @"(eq? #\a #\a)" },
//                { "#f", @"(eq? 1 1)" },
//                { "#f", "(eq? \"a string\" \"a string\")" },
//                { "#f", "(eq? #(1 2 3) #(1 2 3))" },
//                { "#f", "(eq? '(1 . 2) '(1 . 2))" }
//            });

//            [TestMethod]
//            public void EvalEqv() => Tester.TestBlock(new()
//            {
//                { "#t", "(eqv? '() '())" },
//                { "#t", "(eqv? 'a 'a)" },
//                { "#t", "(eqv? + +)" },
//                { "#t", @"((lambda (x) (eqv? x x)) #\a)" },
//                { "#t", @"(eqv? #\a #\a)" },
//                { "#t", @"(eqv? 1 1)" },
//                { "#f", "(eqv? \"a string\" \"a string\")" },
//                { "#f", "(eqv? #(1 2 3) #(1 2 3))" },
//                { "#f", "(eqv? '(1 . 2) '(1 . 2))" }
//            });

//            [TestMethod]
//            public void EvalEqual() => Tester.TestBlock(new()
//            {
//                { "#t", "(equal? '() '())" },
//                { "#t", "(equal? 'a 'a)" },
//                { "#t", "(equal? + +)" },
//                { "#t", @"((lambda (x) (equal? x x)) #\a)" },
//                { "#t", @"(equal? #\a #\a)" },
//                { "#t", @"(equal? 1 1)" },
//                { "#t", "(equal? \"a string\" \"a string\")" },
//                { "#t", "(equal? #(1 2 3) #(1 2 3))" },
//                { "#t", "(equal? '(1 . 2) '(1 . 2))" }
//            });


//        }
//    }

//    //[TestClass]
//    //public class SpecialForms
//    //{
//    //    [TestClass]
//    //    public class If
//    //    {
//    //        [TestMethod]
//    //        public void CaseTrue() => Tester.TestIO("1", "(if 1 1 2)");

//    //        [TestMethod]
//    //        public void CaseFalse() => Tester.TestIO("2", "(if () 1 2)");

//    //        [TestMethod]
//    //        public void SubConditional() => Tester.TestIO("1", "(if (if 1 1 ()) 1 2)");

//    //        [TestMethod]
//    //        public void SubConsequent() => Tester.TestIO("5", "(if () 1 (+ 2 3))");

//    //        [TestMethod]
//    //        public void SelectiveEvaluation()
//    //        {
//    //            string test = "(begin (define var 0) (if () (set! var 1) (set! var 2)) var)";
//    //            Tester.TestIO("2", test);
//    //        }
//    //    }

//    [TestClass]
//    public class Cond
//    {
//        //[TestMethod]
//        //public void CaseSimpleFirst() => Tester.TestIO("1", "(cond (#t 1) (#t 3))");

//        //[TestMethod]
//        //public void CaseSimpleSecond() => Tester.TestIO("3", "(cond (#f 1) (#t 3))");

//        //[TestMethod]
//        //public void CaseComplexSecond() => Tester.TestIO("3", "(cond (#f 1) (#t 3) (#t 5))");

//        //[TestMethod]
//        //public void CaseThird() => Tester.TestIO("5", "(cond (#f 1) (#f 3) (#t 5))");

//        //[TestMethod]
//        //public void SubConditionalTrue() => Tester.TestIO("1", "(cond ((if 1 1 ()) 1) (2 3))");

//        //[TestMethod]
//        //public void SubConditionalFalse() => Tester.TestIO("3", "(cond ((if 1 () 1) 1) (2 3))");

//        //[TestMethod]
//        //public void SubConsequent() => Tester.TestIO("11", "(cond (0 (if () 10 11)) (2 3))");

//        //[TestMethod]
//        //public void SelectiveEvaluation()
//        //{
//        //    string test = "(begin (define var 0) (cond (() (set! var 1)) ((define new-var var) new-var)))";
//        //    Tester.TestIO("0", test);
//        //}
//    }

//    //    [TestClass]
//    //    public class Quote
//    //    {
//    //        [TestMethod]
//    //        public void QuoteSymbol() => Tester.TestIO("a", "(quote a)");
//    //        [TestMethod]
//    //        public void QuoteProcedure() => Tester.TestIO("(+ 1 2)", "(quote (+ 1 2))");

//    //        [TestMethod]
//    //        public void QuoteMarkSymbol() => Tester.TestIO("a", "'a");
//    //        [TestMethod]
//    //        public void QuoteMarkEmpty() => Tester.TestIO("()", "'()");
//    //        [TestMethod]
//    //        public void QuoteMarkProcedure() => Tester.TestIO("(+ 1 2)", "'(+ 1 2");
//    //        [TestMethod]
//    //        public void QuoteMarkQuoteSymbol() => Tester.TestIO("(quote a)", "'(quote a)");
//    //        [TestMethod]
//    //        public void QuoteMarkQuoteMarkSymbol() => Tester.TestIO("(quote a)", "''a");

//    //        [TestMethod]
//    //        public void QuoteSelfString() => Tester.TestIO("\"abc\"", "'\"abc\"");
//    //        [TestMethod]
//    //        public void QuoteSelfNumber() => Tester.TestIO("31415", "'31415");
//    //        [TestMethod]
//    //        public void QuoteSelfBoolean() => Tester.TestIO("#t", "'#t");
//    //    }



//    //}
//}
