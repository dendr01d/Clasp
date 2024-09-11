
;; ----------------------------------------------------------------------------
;; Car/Cdr extensions
(define (caar ls) (car (car ls)))
(define (cadr ls) (car (cdr ls)))
(define (cdar ls) (cdr (car ls)))
(define (cddr ls) (cdr (cdr ls)))

(define (caaar ls) (car (caar ls)))
(define (caadr ls) (car (cadr ls)))
(define (cadar ls) (car (cdar ls)))
(define (caddr ls) (car (cddr ls)))

(define (cdaar ls) (cdr (caar ls)))
(define (cdadr ls) (cdr (cadr ls)))
(define (cddar ls) (cdr (cdar ls)))
(define (cdddr ls) (cdr (cddr ls)))

;; ----------------------------------------------------------------------------
;; Derivative Predicates

(define (true? x) x)
(define (false? x) (if x #f #t))

(define (zero? x) (eqv? x zero))

(define (positive? x) (> x zero))
(define (negative? x) (< x zero))

(define (odd? x) (eqv? (mod x 2) one))
(define (even? x) (eqv? (mod x 2) zero))

;; ----------------------------------------------------------------------------
;; Derivative Logical Operations

(define (not x) (false? x))
(define (impl x y) (or (not a) b))
(define (bimpl a b) (and (impl a b) (impl b a)))
(define (xor a b) (or (and (not a) b) (and a (not b))))

;; ----------------------------------------------------------------------------
;; Mathematical Constants

(define zero 0)
(define one 1)
(define pi 3.1415926536)
(define e 2.7182818285)
(define phi 1.6180339887)


;; ----------------------------------------------------------------------------
;; Math Ops

(define (= x y)
	(and (number? x)
		 (number? y)
		 (eqv? x y)))
		 
(define / quotient)
(define (// number root) (expt number (quotient one root)))
(define (sqrt n) (// n 2))
(define (cbrt n) (// n 3))

(define (min x . xs)
	(reduce (lambda (a b) (if (< a b) a b)) x xs))
	
(define (max x . xs)
	(reduce (lambda (a b) (if (> a b) a b)) x xs))
	
(define (inc n) (+ n one))
(define (dec n) (- n one))

;; ----------------------------------------------------------------------------
;; General List Ops

(define (list . args) args)

(define (list* a b . more)
  (if (null? more)
      (cons a b)
      (cons a (list* b . more))))

(define (mapcar op ls)
	(if (null? ls)
	'()
	(cons (op (car ls))
	      (mapcar op (cdr ls)))))
		  
(define (foldr op init ls)
	(if (null? ls)
		init
		(op (car ls)
			(foldr op init (cdr ls)))))
		  
(define (foldl op init ls)
	(if (null? ls)
		init
		(foldl op
			   (op (car ls) init)
			   (cdr ls))))
			   
(define (fold op init ls)
  (foldl op init ls))
  
(define (reduce op init ls)
  (foldl op init ls))
  
(define (reverse ls)
	(if (null? ls)
	    '()
		(append (reverse (cdr ls))
		        (list (car ls)))))

;; ----------------------------------------------------------------------------
;; List Membership

(define (memp obj ls comp)
	(cond
		((null? ls) #f)
		((comp obj (car ls)) #t)
		(else (memp obj (cdr ls) comp))))
		
(define (memq obj ls) (memp obj ls eq?))
(define (memv obj ls) (memp obj ls eqv?))
(define (member obj ls) (memp obj ls equal?))

;; ----------------------------------------------------------------------------
;; Assoc-List Ops

(define (assp obj alist compare)
	(cond
		((null? alist) #f)
		((compare obj (caar alist)) (car alist))
		((else (assp obj (cdr alist) compare)))))
		
(define (assq obj alist) (assp obj alist eq?))
(define (assv obj alist) (assp obj alist eqv?))
(define (assoc obj alist) (assp obj alist equal?))

;; ----------------------------------------------------------------------------
;; Random Tests

; http://peter.michaux.ca/articles/scheme-from-scratch-bootstrap-v0_13-lambda-the-ultimate
(define Y
    (lambda (f)
      ((lambda (x) (f (lambda (y) ((x x) y))))
       (lambda (x) (f (lambda (y) ((x x) y)))))))
	   
(define factorial
    (Y (lambda (fact) 
         (lambda (n) 
           (if (= n 0)
               1
               (* n (fact (- n 1))))))))

(define insert-into-sorted
  (lambda (item sorted comp)
          (cond ((null? sorted) (list item))
                ((comp item (car sorted)) (cons item sorted))
                (else (cons (car sorted)
                            (insert-into-sorted item (cdr sorted) comp))))))

(define insert-all
  (lambda (input sorted comp)
          (if (null? input)
              sorted
              (insert-all (cdr input)
                          (insert-into-sorted (car input) sorted comp)
                          comp))))

(define insertion-sort
  (lambda (ls)
          (insert-all ls '() <)))

;; ----------------------------------------------------------------------------
;; Standard Macros

(defmacro when ()
	((_ test result)
		(if test result #f))
	((_ test)
		test))
        
; (defmacro unless ()
    ; ((_ test body1 body2 ...)
        ; (let ((check test)) (if check check (begin body1 body2 ...)))))
		
(defmacro let ()
	((_ ((keys vals) ...) body1 body2 ...)
		((lambda (keys ...) body1 body2 ...) vals ...)))
		
(defmacro let* ()
	((_ () body1 . body2)
		(begin body1 . body2))
	((_ ((key val) more ...) body1 . body2)
		((lambda (key) (let* (more ...) body1 . body2)) val)))
		
(defmacro letrec ()
	((_ ((keys vals) ...) body1 body2 ...)
		(begin
			(define keys vals) ...
			body1
			body2 ...)))
			
(defmacro letrec* ()
	((_ () body1 body2 ...)
		(begin body1 body2 ...))
	((_ ((key1 val1) (keys vals) ...) body1 body2 ...)
		(begin
			(define key1 val1)
			(letrec* ((keys vals) ...) body1 body2 ...))))
			
(defmacro or ()
	((_ arg)
		arg)
	((_ arg . args)
		((lambda (x) (if x x (or . args))) arg))
	((_)
		#f))
		
(defmacro and ()
	((_ arg)
		arg)
	((_ arg . args)
		((lambda (x) (if x (and . args) x)) arg))
	((_)
		#t))
		
(defmacro cond (else)
	((_ (else body1 . body2))
		(begin body1 . body2))
    ; ((_ (test => proc) clauses ...)
        ; ((lambda (x y) (if x (y x) (cond clauses ...))) test proc))
	((_ (test body1 . body2) . clauses)
		(if test
			(begin body1 . body2)
			(cond . clauses)))
	((_ (test))
		test)
	((_)
		#f))
    
; (defmacro case (else)
    ; ((_ (else body1 body2 ...))
        ; (begin body1 body2 ...))
    ; ((_ target ((item1 item2 ...) body1 body2 ...) clauses ...)
        ; (if (member target (item1 item2 ...)) (begin body1 body2 ...) (case target clauses ...)))
    ; ((_)
        ; #f))