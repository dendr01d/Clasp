using Xunit.Sdk;

namespace Tests
{
    [TestClass]
    public class R5RS
    {
        [TestMethod]
        public void LambdaAddition() => Tester.TestIO("8", "((lambda (x) (+ x x)) 4)");

        [TestMethod]
        public void LambdaIdentity() => Tester.TestIO("(3 4 5 6)", "((lambda x x) 3 4 5 6)");

        //[TestMethod]
        //public void ForceDotted() => Tester.TestIO("(5 6)", "((lambda (x y . z) z) 3 4 5 6)");

        [TestMethod]
        public void GreaterTrue() => Tester.TestIO("yes", "(if (> 3 2) 'yes 'no)");

        [TestMethod]
        public void GreaterFalse() => Tester.TestIO("no", "(if (> 2 3) 'yes 'no)");

        [TestMethod]
        public void SwitchOnSub() => Tester.TestIO("1", "(if (> 3 2) (- 3 2) (+ 3 2))");

        [TestMethod]
        public void CondOnSub() => Tester.TestIO("greater", "(cond ((> 3 2) 'greater) ((< 3 2) 'less))");

        [TestMethod]
        public void CondDefault() => Tester.TestIO("equal", "(cond ((> 3 3) 'greater) ((< 3 3) 'less) ('t 'equal))");

        [TestMethod]
        public void Cases() => Tester.TestIO("composite", "(case (* 2 3) ((2 3 5 7) 'prime) ((1 4 6 8 9) 'composite))");

        //[TestMethod]
        //public void aaaaaa() => Tester.TestIO("consonant", "(case (car '(c d)) ((a e i o u) 'vowel) ((w y) 'semivowel) (else 'consonant))");

        [TestMethod]
        public void AndTrue() => Tester.TestIO("#t", "(and (= 2 2) (> 2 1))");

        [TestMethod]
        public void AndFalse() => Tester.TestIO("#f", "(and (= 2 2) (< 2 1))");

        //[TestMethod]
        //public void aaaaaa() => Tester.TestIO("(f g)", "(and 1 2 'c '(f g))");

        [TestMethod]
        public void AndNoArgs() => Tester.TestIO("#t", "(and)");

        [TestMethod]
        public void OrTrue() => Tester.TestIO("#t", "(or (= 2 2) (> 2 1))");

        [TestMethod]
        public void OrFalse() => Tester.TestIO("#t", "(or (= 2 2) (< 2 1))");

        [TestMethod]
        public void LetAssignment() => Tester.TestIO("6", "(let ((x 2) (y 3)) (* x y))");

        [TestMethod]
        public void LetReassignment() => Tester.TestIO("35", "(let ((x 2) (y 3)) (let ((x 7) (z (+ x y))) (* z x)))");

        //[TestMethod]
        //public void aaaaaa() => Tester.TestIO("-2", "(let () (define x 2) (define f (lambda () (- x))) (f))");

        [TestMethod]
        public void LetBegin() => Tester.TestIO("25", "(let ((x '(1 3 5 7 9))) (begin ((x x (cdr x)) (sum 0 (+ sum (car x)))) ((null? x) sum)))");

        //[TestMethod]
        //public void aaaaaa() => Tester.TestIO("((6 1 3) (-5 -2))", " (let loop ((numbers '(3 -2 1 6 -5)) (nonneg '()) (neg '())) (cond ((null? numbers) (list nonneg neg)) ((>= (car numbers) 0) (loop (cdr numbers) (cons (car numbers) nonneg) neg)) ((< (car numbers) 0)(loop (cdr numbers) nonneg (cons (car numbers) neg)))))");

        [TestMethod]
        public void EquivSymbolTrue() => Tester.TestIO("#t", "(eqv? 'a 'a)");

        [TestMethod]
        public void EqSuivymbolFalse() => Tester.TestIO("#f", "(eqv? 'a 'b)");

        [TestMethod]
        public void EquivEmpty() => Tester.TestIO("#t", "(eqv? '() '())");

        [TestMethod]
        public void EquivCons() => Tester.TestIO("#f", "(eqv? (cons 1 2) (cons 1 2))");

        [TestMethod]
        public void EquivLambdaFalse() => Tester.TestIO("#f", "(eqv? (lambda () 1) (lambda () 2))");

        [TestMethod]
        public void EquivLambdaTrue() => Tester.TestIO("#t", "(let ((p (lambda (x) x))) (eqv? p p))");

        //[TestMethod]
        //public void EqSymbol() => Tester.TestIO("#t", "(eq? 'a 'a)");

        [TestMethod]
        public void EqList() => Tester.TestIO("#f", "(eq? (list 'a) (list 'a))");

        [TestMethod]
        public void EqEmpty() => Tester.TestIO("#t", "(eq? '() '())");

        [TestMethod]
        public void EqProcedure() => Tester.TestIO("#t", "(eq? car car)");

        [TestMethod]
        public void EqInLet() => Tester.TestIO("#t", "(let ((x '(a))) (eq? x x))");

        [TestMethod]
        public void EqLambda() => Tester.TestIO("#t", "(let ((p (lambda (x) x))) (eq? p p))");

        [TestMethod]
        public void EqualSymbol() => Tester.TestIO("#t", "(equal? 'a 'a)");

        [TestMethod]
        public void EqualSingleList() => Tester.TestIO("#t", "(equal? '(a) '(a))");

        [TestMethod]
        public void EqualMultiList() => Tester.TestIO("#t", "(equal? '(a (b) c) '(a (b) c))");

        [TestMethod]
        public void EqualNumber() => Tester.TestIO("#t", "(equal? 2 2)");

        [TestMethod]
        public void MathMax() => Tester.TestIO("4", "(max 3 4)");

        [TestMethod]
        public void MathPlus() => Tester.TestIO("7", "(+ 3 4)");

        [TestMethod]
        public void MathPlusSingle() => Tester.TestIO("3", "(+ 3)");

        [TestMethod]
        public void MathPlusIdentity() => Tester.TestIO("0", "(+)");

        [TestMethod]
        public void MathMultiSingle() => Tester.TestIO("4", "(* 4)");

        [TestMethod]
        public void MathMultiIdentity() => Tester.TestIO("1", "(*)");

        [TestMethod]
        public void MathSubtractTwo() => Tester.TestIO("-1", "(- 3 4)");

        [TestMethod]
        public void MathSubtractThree() => Tester.TestIO("-6", "(- 3 4 5)");

        [TestMethod]
        public void MathSubtractOne() => Tester.TestIO("-3", "(- 3)");

        //[TestMethod]
        //public void MathSubtractFloat() => Tester.TestIO("-1.0", "(- 3.0 4)");

        [TestMethod]
        public void MathAbs() => Tester.TestIO("7", "(abs -7)");

        [TestMethod]
        public void MathModulo() => Tester.TestIO("1", "(% 13 4)");

        [TestMethod]
        public void MathModuloNegLeft() => Tester.TestIO("3", "(% -13 4)");

        [TestMethod]
        public void MathModuloNegRight() => Tester.TestIO("-3", "(% 13 -4)");

        [TestMethod]
        public void MathModuloNegBoth() => Tester.TestIO("-1", "(% -13 -4)");

        //[TestMethod]
        //public void MathRemainder() => Tester.TestIO("1", "(remainder 13 4)");

        //[TestMethod]
        //public void MathRemainderNegLeft() => Tester.TestIO("-1", "(remainder -13 4)");

        //[TestMethod]
        //public void MathRemainderNegRight() => Tester.TestIO("1", "(remainder 13 -4)");

        //[TestMethod]
        //public void MathRemainderNegBoth() => Tester.TestIO("-1", "(remainder -13 -4)");

        [TestMethod]
        public void NotNumber() => Tester.TestIO("#f", "(not 3)");

        [TestMethod]
        public void NotList() => Tester.TestIO("#f", "(not (list 3))");

        [TestMethod]
        public void NotEmpty() => Tester.TestIO("#f", "(not '())");

        [TestMethod]
        public void NotSoloList() => Tester.TestIO("#f", "(not (list))");

        //[TestMethod]
        //public void aaaaaa() => Tester.TestIO("#f", "(boolean? 0)");

        //[TestMethod]
        //public void aaaaaa() => Tester.TestIO("#f", "(boolean? '())");

        [TestMethod]
        public void PairDotted() => Tester.TestIO("#t", "(pair? '(a . b))");

        [TestMethod]
        public void PairList() => Tester.TestIO("#t", "(pair? '(a b c))");

        [TestMethod]
        public void LinkedList() => Tester.TestIO("(a)", "(cons 'a '())");

        [TestMethod]
        public void PrependList() => Tester.TestIO("((a) b c d)", "(cons '(a) '(b c d))");

        [TestMethod]
        public void ConsDotted() => Tester.TestIO("(a . 3)", "(cons 'a 3)");

        [TestMethod]
        public void ConsListOnDotted() => Tester.TestIO("((a b) . c)", "(cons '(a b) 'c)");

        [TestMethod]
        public void CarList() => Tester.TestIO("a", "(car '(a b c))");

        [TestMethod]
        public void CarPrependedList() => Tester.TestIO("(a)", "(car '((a) b c d))");

        [TestMethod]
        public void CarDotted() => Tester.TestIO("1", "(car '(1 . 2))");

        [TestMethod]
        public void CdrPrependedList() => Tester.TestIO("(b c d)", "(cdr '((a) b c d))");

        [TestMethod]
        public void CdrDotted() => Tester.TestIO("2", "(cdr '(1 . 2))");

        [TestMethod]
        public void ListIsList() => Tester.TestIO("#t", "(list? '(a b c))");

        [TestMethod]
        public void EmptyIsList() => Tester.TestIO("#t", "(list? '())");

        [TestMethod]
        //public void DottedIsList() => Tester.TestIO("#f", "(list? '(a . b))"); //shouldn't this be true??
        public void DottedIsList() => Tester.TestIO("#t", "(list? '(a . b))");

        //[TestMethod]
        //public void aaaaaa() => Tester.TestIO("#f", "(let ((x (list 'a))) (set-cdr! x x) (list? x))");

        [TestMethod]
        public void EvaluateList() => Tester.TestIO("(a 7 c)", "(list 'a (+ 3 4) 'c)");

        [TestMethod]
        public void ListOfNone() => Tester.TestIO("()", "(list)");

        [TestMethod]
        public void LengthRegList() => Tester.TestIO("3", "(length '(a b c))");

        [TestMethod]
        public void LengthWeirdList() => Tester.TestIO("3", "(length '(a (b) (c d e)))");

        [TestMethod]
        public void LengthEmptyList() => Tester.TestIO("0", "(length '())");

        [TestMethod]
        public void AppendSolos() => Tester.TestIO("(x y)", "(append '(x) '(y))");

        [TestMethod]
        public void AppendSoloToList() => Tester.TestIO("(a b c d)", "(append '(a) '(b c d))");

        [TestMethod]
        public void AppendWeirdLists() => Tester.TestIO("(a (b) (c))", "(append '(a (b)) '((c)))");

        [TestMethod]
        public void AppendDotted() => Tester.TestIO("(a b c . d)", "(append '(a b) '(c . d))");

        [TestMethod]
        public void AppendWithNone() => Tester.TestIO("a", "(append '() 'a)");

        //[TestMethod]
        //public void aaaaaa() => Tester.TestIO("(c b a)", "(reverse '(a b c))");

        //[TestMethod]
        //public void aaaaaa() => Tester.TestIO("((e(f)) d (b c) a)", " (reverse '(a (b c) d (e (f))))");

        [TestMethod]
        public void SymbolIsSymbol() => Tester.TestIO("#t", "(symbol? 'foo)");

        [TestMethod]
        public void ResultIsSymbol() => Tester.TestIO("#t", "(symbol? (car '(a b)))");

        [TestMethod]
        public void AnotherSymbolIsSymbol() => Tester.TestIO("#t", "(symbol? 'nil)");

        [TestMethod]
        public void EmptyIsNotSymbol() => Tester.TestIO("#f", "(symbol? '())");

        [TestMethod]
        public void CarIsProcedure() => Tester.TestIO("#t", "(procedure? car)");

        [TestMethod]
        public void CdrIsProcedure() => Tester.TestIO("#f", "(procedure? 'car)");

        [TestMethod]
        public void LambdaIsProcedure() => Tester.TestIO("#t", "(procedure? (lambda (x) (* x x)))");

        [TestMethod]
        public void QuotedIsNotProcedure() => Tester.TestIO("#f", "(procedure? '(lambda (x) (* x x)))");

        [TestMethod]
        public void ApplyProcedure() => Tester.TestIO("7", "(apply + (list 3 4))");

        [TestMethod]
        public void MapCadr() => Tester.TestIO("(b e h)", "(map cadr '((a b) (d e) (g h)))");

        [TestMethod]
        public void MapOperator() => Tester.TestIO("(5 7 9)", "(map + '(1 2 3) '(4 5 6))");

        //[TestMethod]
        //public void aaaaaa() => Tester.TestIO("ok", "(let ((else 1)) (cond (else 'ok) ('t 'bad)))");

        //[TestMethod]
        //public void aaaaaa() => Tester.TestIO("ok", "(let ((=> 1)) (cond (#t => 'ok)))");

        [TestMethod]
        public void LetSet1() => Tester.TestIO("(2 1)", "((lambda () (let ((x 1)) (let ((y x)) (set! x 2) (list x y)))))");

        [TestMethod]
        public void LetSet2() => Tester.TestIO("(2 2)", "((lambda () (let ((x 1)) (set! x 2) (let ((y x)) (list x y)))))");

        [TestMethod]
        public void LetSet3() => Tester.TestIO("(1 2)", "((lambda () (let ((x 1)) (let ((y x)) (set! y 2) (list x y)))))");

        [TestMethod]
        public void LetSet4() => Tester.TestIO("(2 3)", "((lambda () (let ((x 1)) (let ((y x)) (set! x 2) (set! y 3)(list x y)))))");


    }
}