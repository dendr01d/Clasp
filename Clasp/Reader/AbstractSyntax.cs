using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Primitives;

namespace Clasp.Reader
{
    //From the paper "Macros that work Together"
    // by Flatt, Culpepper, Darais, Findler

    /*
     * Ast :
     *     | Var(Name)
     *     | App(Op, Args...)
     *     | Val
     * 
     * Val :
     *     | Fun(Body, Parameters)
     *     | List(Elements)
     *     | Atom
     *     | Stx
     * 
     * Atom :
     *      | Sym('Name)
     *      | Prim(Primitive)
     *      | Literal
     *      
     * Stx :
     *     | Stx(Atom, Ctx)
     *     | Stx(List, Ctx)
     *     | Id
     *     
     * Id :
     *    | Stx(Sym, Ctx)
     *    
     * Ctx
     * 
     */

    internal abstract record Ast
    {
        public abstract Ast Eval(Ctx context);

        public IEnumerable<Ast> EvalMany(Ctx context, params Ast[] terms)
        {
            return terms.Select(x => x.Eval(context));
        }

        public override string ToString() => Print();
        protected abstract string Print();
    }

    internal sealed record Var(string Name) : Ast
    {
        protected override string Print() => string.Format("VAR({0})", Name);
    }
    internal sealed record App(Ast Op, params Ast[] Args) : Ast
    {
        public override Ast Eval(Ctx context)
        {
            return Op switch
            {
                Fun f => f.Body.Eval(context.BindMany(f.Parameters, Args)),
                Prim p => p.DeltaRelate(this, EvalMany(context, Args).ToArray()),
                _ => new App(Op.Eval(context), Args).Eval(context)
            };
        }

        protected override string Print() => string.Format("APP({0}, [{1}])", Op, string.Join(", ", Args.AsEnumerable()));
    }

    internal abstract record Val : Ast
    {
        public override Ast Eval(Ctx context) => this;
    }
    internal sealed record Fun(Ast Body, params Var[] Parameters) : Val
    {
        protected override string Print() => string.Format("FUN([{0}], {1})", string.Join(", ", Parameters.AsEnumerable()), Body);
    }
    internal sealed record List(Val[] Elements, bool NullTerminated = true) : Val
    {
        protected override string Print() => string.Format("LIST({0})",
            string.Join(", ", Elements.AsEnumerable()));
    }

    internal abstract record Atom : Val { }
    internal sealed record Sym(string QuotedName) : Atom
    {
        protected override string Print() => string.Format("'{0}", QuotedName);
    }
    internal sealed record Prim(Primitive Type) : Atom
    {
        public Ast DeltaRelate(App caller, params Ast[] args)
        {
            return (Type, args) switch
            {
                (Primitive.CONS, [Val v1, List l]) => new List(v1, l),
                (Primitive.CONS, _) => throw new Exception(),

                (Primitive.CAR, [List l]) when l.Elements.Length > 1 => l.Elements[0],
                (Primitive.CAR, _) => throw new Exception(),

                (Primitive.CDR, [List l]) when l.Elements.Length > 1 => new List(l.Elements[1..]),
                (Primitive.CDR, _) => throw new Exception(),

                (Primitive.LIST, Val[] vals) => new List(vals),
                (Primitive.LIST, _) => throw new Exception(),

                (Primitive.SYNTAX_E, [Stx stx]) => stx.Data,
                (Primitive.SYNTAX_E, _) => throw new Exception(),

                (Primitive.MK_SYNTAX, [Atom a, Stx stx]) => new Stx(a, stx.Context),
                (Primitive.MK_SYNTAX, [List l, Stx stx]) when l.Elements is Stx[] stxs => new Stx(l, stx.Context),
                (Primitive.MK_SYNTAX, _) => throw new Exception(),

                (_, _) => caller
            };
        }

        protected override string Print() => string.Format("<{0}>", Type.ToString());
    }
    internal sealed record Literal(Lexer.TokenType TokenType, string TokenText) { }

    internal record Stx(Val Data, Ctx Context, int Line, int Index) : Val
    {
        protected override string Print() => string.Format("STX({0}, {1})", Data, Context);
    }
    //internal sealed record Stx_Atom(Atom Atom, Ctx Context) : Stx(Atom, Context) { }
    //internal sealed record Stx_List(Ctx Context, params Stx[] Elements) : Stx(new List(Elements), Context) { }

    internal sealed record Id(Sym Sym, Ctx Context, int Line, int Index) : Stx(Sym, Context, Line, Index) { }

    internal class Ctx
    {
        public Ctx Bind(Var key, Ast value)
        {
            throw new NotImplementedException();
        }

        public Ctx BindMany(IEnumerable<Var> keys, IEnumerable<Ast> values)
        {
            //TODO does this properly throw an error if the collections have different counts?
            foreach(var pair in keys.Zip(values))
            {
                Bind(pair.First, pair.Second);
            }

            return this;
        }

        public override string ToString() => "∙";
    }

}
