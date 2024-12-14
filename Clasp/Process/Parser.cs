using System;
using System.Reflection;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.ConcreteSyntax;
using Clasp.Data.Terms;

namespace Clasp.Process
{
    internal static class Parser
    {
        public static AstNode ParseAST(Syntax stx)
        {
            return ParseAST(stx, new Binding.BindingStore());
        }

        public static AstNode ParseAST(Term term, Binding.BindingStore bs)
        {
            return term switch
            {
                SyntaxId sid => ParseIdentifier(sid, bs),
                SyntaxList sp => ParseProduct(sp, bs),
                SyntaxAtom sat => new Fixed(sat.WrappedValue),
                Term t => new Fixed(t),
                _ => throw new ParserException("Cannot parse form: ", term)
            };
        }

        private static Var ParseIdentifier(SyntaxId id, Binding.BindingStore bs)
        {
            string varName = bs.ResolveName(id.WrappedValue.Name, id.Context);
            return new Var(varName);
        }

        private static AstNode ParseProduct(SyntaxList prod, Binding.BindingStore bs)
        {
            //if (prod.WrappedValue is Vector vec)
            //{
            //    return ParseVector(vec, bs);
            //}
            //else
            if (prod.WrappedValue is ConsCell cell)
            {
                return ParseApplicative(cell, bs);
            }

            throw new ParserException("Unknown syntax.", prod);
        }

        //private static Fixed ParseVector(Vector vec, Binding.BindingStore bs)
        //{
        //    List<Fixed> contents = new List<Fixed>();
        //    int index = 0;

        //    foreach (Term value in vec.Values)
        //    {
        //        AstNode parsed = ParseAST(value, bs);

        //        if (parsed is Fixed parsedValue)
        //        {
        //            contents.Add(parsedValue);
        //        }
        //        else
        //        {
        //            throw new ParserException(
        //                "Expected vector element {0} at index {1} to parse to fixed value.",
        //                value,
        //                index);
        //        }

        //        ++index;
        //    }

        //    return new Vector(contents.ToArray());
        //}

        private static AstNode ParseApplicative(ConsCell cell, Binding.BindingStore bs)
        {
            //the only valid way to start a list is with a proc or otherwise not-fixed gennode
            //buuuut procs technically can't be represented syntactically
            //primitives are indirectly accessed by reference
            //and compounds can only be constructed during runtime
            //so only non-fixed gennodes are allowed

            AstNode parsedCar = ParseAST(cell.Car, bs);

            if (parsedCar is not Generative genCar)
            {
                throw new ParserException("Leading term of new applicative form isn't generative: ", parsedCar);
            }
            else if (genCar is Fixed fixCar)
            {
                throw new ParserException("Leading term of new applicative form is a fixed value: {0}", fixCar);
            }
            else
            {
                FlatList<AstNode> tail = FlatList<AstNode>.FromNested(cell.Cdr)
                    ?? throw new ParserException("Couldn't flatten args to applicative form: {0}", cell);

                if (tail.IsDotted)
                {
                    throw new ParserException("Implicit applicative form was given dotted argument list: {0}", tail);
                }
                else if (genCar is Functional funCar)
                {
                    // Application of dynamically constructed compound proc
                    return ParseApplication(funCar, tail, bs);
                }
                else if (genCar is Var varCar)
                {
                    // Dispatch and potentially format a special form
                    return ParseTaggedForm(varCar, tail, bs);
                }
                else
                {
                    throw new ParserException.UnknownSyntax(parsedCar);
                }
            }
        }

        private static FlatList<AstNode> ParseNestedList(AstNode node, Binding.BindingStore bs)
        {
            throw new NotImplementedException();
        }

        private static AstNode ParseTaggedForm(Var car, FlatList<AstNode> tail, Binding.BindingStore bs)
        {
            if (car.Name == Symbol.Quote.Name)
            {
                return ParseQuote(tail);
            }
            else if (car.Name == Symbol.Syntax.Name)
            {
                return ParseSyntaxForm(tail);
            }
            else if (car.Name == Symbol.Lambda.Name)
            {
                return ParseFun(tail, bs);
            }
            else if (car.Name == Symbol.If.Name)
            {
                return ParseBranch(tail, bs);
            }
            else if (car.Name == Symbol.Begin.Name)
            {
                return ParseSequence(tail, bs);
            }
            else if (car.Name == Symbol.Define.Name)
            {
                return ParseDefineFixed(tail, bs);
            }
            else if (car.Name == Symbol.DefineSyntax.Name)
            {
                return ParseDefineSyntax(tail, bs);
            }
            else if (car.Name == Symbol.Set.Name)
            {
                return ParseSetFixed(tail, bs);
            }
            else
            {
                return ParseApplication(car, tail);
            }
        }

        private static Fixed ParseQuote(FlatList<AstNode> args)
        {
            if (args.LeadingCount != 1)
            {
                throw new ParserException.WrongArity(Symbol.Quote.Name, 1, true, args);
            }
            else if (args[0] is not Fixed fixedArg)
            {
                throw new ParserException.WrongArgType(Symbol.Quote.Name, nameof(Fixed), args);
            }
            else
            {
                return fixedArg is Syntax stx
                    ? stx.Strip()
                    : fixedArg;
            }
        }

        private static Fixed ParseSyntaxForm(FlatList<AstNode> args)
        {
            //if (args.LeadingCount != 1)
            //{
            //    throw new ParserException.WrongArity(Symbol.Syntax.Name, 1, true, args);
            //}
            //else if (args[0] is not Fixed fixedArg)
            //{
            //    throw new ParserException.WrongArgType(Symbol.Syntax.Name, nameof(Fixed), args[0]);
            //}
            //else
            //{
            //    return fixedArg is Syntax stx
            //        ? stx
            //        : Syntax.Wrap(fixedArg)
            //}
            throw new NotImplementedException();
        }

        private static Functional ParseFun(FlatList<AstNode> args, Binding.BindingStore bs)
        {
            if (args.LeadingCount < 2)
            {
                throw new ParserException.WrongArity(Symbol.Lambda.Name, 1, false, args);
            }
            else if (args[0] is not ConsCell cell
                || FlatList<Var>.FromNested(cell) is not FlatList<Var> vars)
            {
                throw new ParserException.WrongArgType(Symbol.Lambda.Name, "Var list", 1, args[0]);
            }
            else
            {
                Sequence body = ParseSequence(new FlatList<AstNode>(args.Skip(1)), bs);

                return new Fun(vars, body);
            }
        }

        private static Branch ParseBranch(FlatList<AstNode> args, Binding.BindingStore bs)
        {
            if (args.LeadingCount < 2)
            {
                throw new ParserException.WrongArity(Symbol.If.Name, 2, false, args);
            }
            else if (args.LeadingCount > 3)
            {
                throw new ParserException.WrongArity(Symbol.If.Name, 3, true, args);
            }
            else if (args.Values is not Generative[] gens)
            {
                throw new ParserException.WrongArgType(Symbol.If.Name, nameof(Generative), args);
            }
            else if (args.LeadingCount == 2)
            {
                return new Branch(gens[0], gens[1], Data.Terms.Boolean.False);
            }
            else
            {
                return new Branch(gens[0], gens[1], gens[2]);
            }
        }

        private static Sequence ParseSequence(FlatList<AstNode> args, Binding.BindingStore bs)
        {
            if (args.Values.Last() is not Generative finalGen)
            {
                throw new ParserException(
                    "The '{0}' form must conclude with a generative form, but was given: {1}",
                    Symbol.Begin.Name,
                    args.Values.Last());
            }
            else
            {
                return new Sequence(args.Values, finalGen);
            }
        }

        private static BindFixed ParseDefineFixed(FlatList<AstNode> args, Binding.BindingStore bs)
        {
            throw new NotImplementedException();
        }

        private static BindSyntax ParseDefineSyntax(FlatList<AstNode> args, Binding.BindingStore bs)
        {
            throw new NotImplementedException();
        }

        private static SetFixed ParseSetFixed(FlatList<AstNode> args, Binding.BindingStore bs)
        {
            throw new NotImplementedException();
        }

        private static Appl ParseApplication(Generative op, AstNode tail, Binding.BindingStore bs)
        {
            if (FlatList<Generative>.FromNested(tail) is FlatList<Generative> args
                && !args.IsDotted)
            {
                return new Appl(op, args.Values);
            }
            else
            {
                throw new ParserException("Given node cannot be tail of applicative form: {0}", tail);
            }
        }
    }
}
