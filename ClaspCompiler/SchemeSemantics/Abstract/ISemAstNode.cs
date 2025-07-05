using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemAstNode : IPrintable
    {
        public uint AstId { get; }
    }

    /*
    
    AST := BOD
         | DEF
         | CMD

    BOD := (DEF* CMD* EXP)

    DEF := (define VAR EXP)
    
    CMD := (set! VAR EXP)
         | EXP

    EXP := APP
         | IFF
         | LAM
         | LIT
         | VAR

    APP := (EXP EXP*)
         | (apply EXP EXP*)

    IFF := (if EXP EXP EXP)

    LAM := (lambda FRM BOD)

    FRM := VAR
         | (VAR*)
         | (VAR* . VAR)

    LIT := <value>
         | (quote <exp>)

    VAR := <var>

    */
}
