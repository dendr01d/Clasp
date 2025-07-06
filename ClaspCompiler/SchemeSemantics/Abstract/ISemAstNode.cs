using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemAstNode : IPrintable
    { }

    /**

    Program := [Body]

    AST-Node := [Definition]
              | [Command]
              | [Sub-Form]

    Command := [Assignment]
             | [Expression]

    Definition := (define [Var] [Expression])

    Assignment := (set! [Var] [Expression])

    Expression := [Application]
                | [Conditional]
                | [Lambda]
                | [Literal]
                | [Var]
                | [Sequence]
                | [Annotated-Expression]

    Annotated-Expression := [Expression]

    Application := (apply [Expression] [Formal-Arguments])

    Conditional := (if [Expression] [Expression] [Expression])

    Lambda := (lambda [Formal-Parameters] [Body])

    Literal := [Value]
             | [Quotation]
             | [Primitive]

    Value := int|bool|etc...

    Quotation := (quote [Expression])

    Primitive := +|-|=|etc...
               | [TypePredicate]

    Type-Predicate := integer?
                    | boolean?
                    | number?
                    | etc...

    Sequence := (begin [Body])

    Sub-Form := [Body]
              | [Formal-Arguments]
              | [Formal-Parameters]

    Body := [Definition]* [Command]* [Expression]

    Formal-Arguments := ()
                      | ([Expression] . [Formal-Arguments])

    Formal-Parameters := ()
                       | [Var]
                       | ([Var] . [Formal-Parameters])

    **/
}
