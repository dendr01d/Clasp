
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

(defmacro let ()
    ((_ () body1 body2 ...)
        (begin body1 body2 ...))
    ((_ ((vars vals) ...) body1 body2 ...)
        ((lambda (vars ...) body1 body2 ...) vals ...)))
        
; (defmacro let* ()
    ; ((_ () body1 body2 ...)
        ; (begin body1 body2 ...))
    ; ((_ ((var1 val1) (vars vals) ...) body1 body2 ...)
        ; ((lambda (var1) (let ((vars vals) ...) body1 body2 ...)) val1)))

; (defmacro letrec ()
    ; ((_ () body1 body2 ...)
        ; (begin body1 body2 ...))
    ; ((_ ((vars vals) ...) body1 body2 ...)
        ; (begin
            ; (define vars vals) ...
            ; body1 body2 ...)))

; (defmacro letrec* ()
    ; ((_ () body1 body2 ...)
        ; (begin body1 body2 ...))
    ; ((_ ((var1 val1) (vars vals) ...) body1 body2 ...)
        ; (begin
            ; (define var1 val1)
            ; (letrec* ((vars vals) ...) body1 body2 ...))))

; (defmacro or ()
    ; ((_ arg)
        ; arg)
    ; ((_ arg . args)
        ; (let ((check arg)) (if check check (or args ...))))
    ; ((_)
        ; #f))

; (defmacro and ()
    ; ((_ arg)
        ; arg)
    ; ((_ arg . args)
        ; (let ((check arg)) (if check (and args ...) check)))
    ; ((_)
        ; #t))

; (defmacro cond (else =>)
    ; ((_ (test) test))
    ; ((_ (else body1 body2 ...))
        ; (begin body1 body2 ...))
    ; ((_ (test => proc) clauses ...)
        ; ((lambda (x y) (if x (y x) (cond clauses ...))) test proc))
    ; ((_ (test body1 body2 ...) clauses ...)
        ; (if test (begin body1 body2 ...) (cons clauses ...)))
    ; ((_)
        ; #f))
    
; (defmacro case (else)
    ; ((_ (else body1 body2 ...))
        ; (begin body1 body2 ...))
    ; ((_ target ((item1 item2 ...) body1 body2 ...) clauses ...)
        ; (if (member target (item1 item2 ...)) (begin body1 body2 ...) (case target clauses ...)))
    ; ((_)
        ; #f))

; (defmacro when ()
    ; ((_ test body1 body2 ...)
        ; (let ((check test)) (if check (begin body1 body2 ...) check))))
        
; (defmacro unless ()
    ; ((_ test body1 body2 ...)
        ; (let ((check test)) (if check check (begin body1 body2 ...)))))