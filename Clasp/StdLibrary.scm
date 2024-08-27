
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

;; List Ops
(define (foldl op init ls)
    (if (null? ls) init (foldl op (op init (car ls)) (cdr ls))))
(define (foldr op init ls)
    (if (null? ls) init (op (car ls) (foldr op init (cdr ls)))))

(define (member t ls)
    (cond
        ((null? ls) #f)
        ((equal? (car ls) t) ls)
        (else (member t (cdr ls)))))

(define (memq t ls)
    (cond
        ((null? ls) #f)
        ((eq? (car ls) t) ls)
        (else (member t (cdr ls)))))

(define (memv t ls)
    (cond
        ((null? ls) #f)
        ((eqv? (car ls) t) ls)
        (else (member t (cdr ls)))))

(define (reverse ls)
    (cons (reverse (cdr ls)) (list (car ls))))
    
(define (append ls t)
    (cond
        ((null? ls) (list t))
        ((null? t) ls)
        (else (cons (car ls) (append (cdr ls) t)))))

(define (mapcar ls fun)
    (if (null? ls) '() (cons (fun (car ls)) (mapcar (cdr ls) fun))))

;; Boolean-Logical Extensions
(define (true? x) (if x #t #f))
(define (false? x) (if x #f #t))
(define (not x) (false? x))

(define (impl x y) (or (not x) y))
(define (bimpl x y) (and (impl x y) (imply y x)))
(define (xor x y) (not (bimpl x y)))


;; Macros for additional special syntax

(defmacro list
    ((_ e1 e2 e3 ...) `(cons ,e1 (list ,e2 ,@e3)))
    ((_ e) `(cons ,e ()))
    ((_) '()))

(defmacro list*
    ((_ e1 e2 e3 ...) `(cons ,e1 (list* ,e2 ,@e3)))
    ((_ e) e)
    ((_) '()))

(defmacro let
    ((_ ((key def) ...) body1 body2 ...)
        `((lambda (,@key) ,body1 ,@body2) ,@def)))
        
(defmacro let*
    ((_ () body1 body2 ...)
        `(begin ,body1 ,@body2)))
    ((_ ((key def) more ...) body1 body2 ...)
        `((lambda (,key) (let* (,@more) ,body1 ,@body2)) ,def))

(defmacro letrec
    ((_ () body1 body2 ...)
        `(begin ,body1 ,@body2))
    ((_ ((keys defs) more ...) body1 body2 ...)
        `((lambda (,key) ((lambda (,key) (letrec* (,@more) ,body1 ,@body2)) ,def)) ,key)))
        
(defmacro letrec*
    ((_ () body1 body2 ...)
        `(begin ,body1 ,@body2))
    ((_ ((key def) more ...) body1 body2 ...)
        `((lambda (,key) ()) undefined)))

(defmacro or
    ((_ e1) e1)
    ((_ e1 e2 ...) `(let ((temp ,e1)) (if temp temp (or ,@e2))))
    ((_) #f))

(defmacro and
    ((_ e1) e1)
    ((_ e1 e2 ...) `(let ((temp ,e1)) (if temp (and ,@e2) temp)))
    ((_) #t))

(defmacro cond
    ((_ (test) test))
    ((_ ('else body1 body2 ...))
        `(begin ,body1 ,@body2))
    ((_ (test body1 body2 ...) clauses ...)
        `(if ,test (begin ,body1 ,@body2) (cond ,@clauses)))
    ((_) #f))
    
(defmacro case
    ((_ x ('else body1 body2 ...))
        `(begin ,body1 ,@body2))
    ((_ x ((p1 p2 ...) body1 body2 ...) clauses ...)
        `(let ((temp ,x)) (if (member temp (,p1 ,@p2)) (begin ,body1 ,@body2) (case temp ,@clauses))))
    ((_) #f))
