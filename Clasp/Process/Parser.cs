using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.ConcreteSyntax;
using Clasp.Data.Terms;

namespace Clasp.Process
{
    internal static class Parser
    {
        // the MOST IMPORTANT thing to remember here is that every syntactic form must break down
        // into ONLY the forms representable by AstNodes

        public static AstNode ParseAST(Syntax stx)
        {
            return ParseAST(stx, new BindingStore());
        }

        public static AstNode ParseAST(Term term, BindingStore bs)
        {
            return term switch
            {
                Identifier sid => ParseIdentifier(sid, bs),
                SyntaxList sp => ParseProduct(sp, bs),
                SyntaxAtom sat => new Quotation(sat.WrappedValue),
                _ => throw new ParserException.Uncategorized("Unknown form: {0}", term)
            };
        }

        private static VariableLookup ParseIdentifier(Identifier id, BindingStore bs)
        {
            //string varName = bs.ResolveBindingName(id.WrappedValue.Name, id.Context);
            return new VariableLookup(id.WrappedValue.Name);
        }

        private static AstNode ParseProduct(SyntaxList prod, Binding.BindingStore bs)
        {
            //if (prod.WrappedValue is Vector vec)
            //{
            //    return ParseVector(vec, bs);
            //}
            //else
            if (prod.WrappedValue is ConsList cell)
            {
                return ParseApplication(cell, bs);
            }

            throw new ParserException.UnknownSyntax(prod);
        }

        ////private static Fixed ParseVector(Vector vec, Binding.BindingStore bs)
        ////{
        ////    List<Fixed> contents = new List<Fixed>();
        ////    int index = 0;

        ////    foreach (Term value in vec.Values)
        ////    {
        ////        AstNode parsed = ParseAST(value, bs);

        ////        if (parsed is Fixed parsedValue)
        ////        {
        ////            contents.Add(parsedValue);
        ////        }
        ////        else
        ////        {
        ////            throw new ParserException(
        ////                "Expected vector element {0} at index {1} to parse to fixed value.",
        ////                value,
        ////                index);
        ////        }

        ////        ++index;
        ////    }

        //    return new Vector(contents.ToArray());
        ////}

        private static bool TryParseSpecialForm(ConsList cell, BindingStore bs,
            [MaybeNullWhen(false)] out AstNode? specialForm)
        {
            specialForm = null;

            if (cell.Car is Symbol keyword)
            {
                if (keyword.Name == Symbol.Define.Name
                    && args)
                {
                    return ParseDefinition(keyword.VarName, args);
                }
                else if (keyword.Name == Symbol.Set.Name)
                {
                    return ParseSet(keyword.Name, args);
                }
                else if (keyword.Name == Symbol.Quote.Name)
                {
                    return ParseQuote(args);
                }
                else if (keyword.Name == Symbol.Lambda.Name)
                {
                    return ParseLambda(args);
                }
                else if (keyword.Name == Symbol.If.Name)
                {
                    return ParseBranch(args);
                }
                else if (keyword.Name == Symbol.Begin.Name)
                {
                    return ParseBegin(args);
                }
            }
        }

        private static AstNode ParseApplication(ConsList cell, Binding.BindingStore bs)
        {
            AstNode parsedCar = ParseAST(cell.Car, bs);

            if (parsedCar is VariableLookup vl)
            {
                if (cell.Cdr is SyntaxList args)
                {
                    if (vl.VarName == Symbol.Define.Name)
                    {
                        return ParseDefinition(vl.VarName, args);
                    }
                    else if (vl.VarName == Symbol.Set.Name)
                    {
                        return ParseSet(vl.VarName, args);
                    }
                    else if (vl.VarName == Symbol.Quote.Name)
                    {
                        return ParseQuote(args);
                    }
                    else if (vl.VarName == Symbol.Lambda.Name)
                    {
                        return ParseLambda(args);
                    }
                    else if (vl.VarName == Symbol.If.Name)
                    {
                        return ParseBranch(args);
                    }
                    else if (vl.VarName == Symbol.Begin.Name)
                    {
                        return ParseBegin(args);
                    }
                }
                else
                {
                    throw new ParserException.UnknownSyntax(cell.Cdr);
                }
            }

            if (parsedCar is BindingDefinition or BindingMutation)
            {
                throw new ParserException.Uncategorized("Can't use imperative binding form as applicative operator.");
            }
            else if (parsedCar is Quotation)
            {
                throw new ParserException.Uncategorized("Can't used quoted value as applicative operator.");
            }

            if (cell.Cdr is SyntaxList sl && sl.WrappedValue is ConsList cl)
            {
                return new FunctionApplication(parsedCar, cl.Select(x => ParseAST(x, bs)).ToArray());
            }
            else
            {
                throw new ParserException.Uncategorized("Function application cannot take the form of a dotted list.");
            }
        }

        private static BindingDefinition ParseDefinition(string varName, SyntaxList args, BindingStore bs)
        {
            if (args.WrappedValue is ConsList cl
                && cl.Car is Syntax arg
                && cl.Cdr is Nil)
            {
                AstNode boundValue = ParseAST(arg, bs);
                return new BindingDefinition(varName, boundValue);
            }
            else
            {
                throw new ParserException.Uncategorized("Wrong type/arg number for '{0}' form.", Symbol.Define.Name);
            }
        }

        private static BindingMutation ParseSet(string varName, SyntaxList args, BindingStore bs)
        {
            if (args.WrappedValue is ConsList cl
                && cl.Car is Syntax arg
                && cl.Cdr is Nil)
            {
                AstNode mutatedValue = ParseAST(arg, bs);
                return new BindingMutation(varName, mutatedValue);
            }
            else
            {
                throw new ParserException.Uncategorized("Wrong type/arg number for '{0}' form.", Symbol.Set.Name);
            }
        }

        private static Quotation ParseQuote(SyntaxList args, BindingStore bs)
        {
            if (args.WrappedValue is ConsList cl
                && cl.Car is Syntax arg
                && cl.Cdr is Nil)
            {
                AstNode mutatedValue = ParseAST(arg, bs);
                return new Quotation(varName, mutatedValue);
            }
            else
            {
                throw new ParserException.Uncategorized("Wrong type/arg number for '{0}' form.", Symbol.Quote.Name);
            }
        }

        private static FunctionCreation ParseLambda(SyntaxList args, BindingStore bs)
        {

        }

        private static ConditionalForm ParseBranch(SyntaxList args, BindingStore bs)
        {

        }

        private static SequentialForm ParseBegin(SyntaxList args, BindingStore bs)
        {

        }

        //private static FlatList<AstNode> ParseNestedList(AstNode node, Binding.BindingStore bs)
        //{
        //    throw new NotImplementedException();
        //}

        //private static AstNode ParseTaggedForm(Var car, FlatList<AstNode> tail, Binding.BindingStore bs)
        //{
        //    if (car.Name == Symbol.Quote.Name)
        //    {
        //        return ParseQuote(tail);
        //    }
        //    else if (car.Name == Symbol.Syntax.Name)
        //    {
        //        return ParseSyntaxForm(tail);
        //    }
        //    else if (car.Name == Symbol.Lambda.Name)
        //    {
        //        return ParseFun(tail, bs);
        //    }
        //    else if (car.Name == Symbol.If.Name)
        //    {
        //        return ParseBranch(tail, bs);
        //    }
        //    else if (car.Name == Symbol.Begin.Name)
        //    {
        //        return ParseSequence(tail, bs);
        //    }
        //    else if (car.Name == Symbol.Define.Name)
        //    {
        //        return ParseDefineFixed(tail, bs);
        //    }
        //    else if (car.Name == Symbol.DefineSyntax.Name)
        //    {
        //        return ParseDefineSyntax(tail, bs);
        //    }
        //    else if (car.Name == Symbol.Set.Name)
        //    {
        //        return ParseSetFixed(tail, bs);
        //    }
        //    else
        //    {
        //        return ParseApplication(car, tail);
        //    }
        //}

        //private static Fixed ParseQuote(FlatList<AstNode> args)
        //{
        //    if (args.LeadingCount != 1)
        //    {
        //        throw new ParserException.WrongArity(Symbol.Quote.Name, 1, true, args);
        //    }
        //    else if (args[0] is not Fixed fixedArg)
        //    {
        //        throw new ParserException.WrongArgType(Symbol.Quote.Name, nameof(Fixed), args);
        //    }
        //    else
        //    {
        //        return fixedArg is Syntax stx
        //            ? stx.Strip()
        //            : fixedArg;
        //    }
        //}

        //private static Fixed ParseSyntaxForm(FlatList<AstNode> args)
        //{
        //    //if (args.LeadingCount != 1)
        //    //{
        //    //    throw new ParserException.WrongArity(Symbol.Syntax.Name, 1, true, args);
        //    //}
        //    //else if (args[0] is not Fixed fixedArg)
        //    //{
        //    //    throw new ParserException.WrongArgType(Symbol.Syntax.Name, nameof(Fixed), args[0]);
        //    //}
        //    //else
        //    //{
        //    //    return fixedArg is Syntax stx
        //    //        ? stx
        //    //        : Syntax.Wrap(fixedArg)
        //    //}
        //    throw new NotImplementedException();
        //}

        //private static Functional ParseFun(FlatList<AstNode> args, Binding.BindingStore bs)
        //{
        //    if (args.LeadingCount < 2)
        //    {
        //        throw new ParserException.WrongArity(Symbol.Lambda.Name, 1, false, args);
        //    }
        //    else if (args[0] is not ConsCell cell
        //        || FlatList<Var>.FromNested(cell) is not FlatList<Var> vars)
        //    {
        //        throw new ParserException.WrongArgType(Symbol.Lambda.Name, "Var list", 1, args[0]);
        //    }
        //    else
        //    {
        //        Sequence body = ParseSequence(new FlatList<AstNode>(args.Skip(1)), bs);

        //        return new Fun(vars, body);
        //    }
        //}

        //private static Branch ParseBranch(FlatList<AstNode> args, Binding.BindingStore bs)
        //{
        //    if (args.LeadingCount < 2)
        //    {
        //        throw new ParserException.WrongArity(Symbol.If.Name, 2, false, args);
        //    }
        //    else if (args.LeadingCount > 3)
        //    {
        //        throw new ParserException.WrongArity(Symbol.If.Name, 3, true, args);
        //    }
        //    else if (args.Values is not Generative[] gens)
        //    {
        //        throw new ParserException.WrongArgType(Symbol.If.Name, nameof(Generative), args);
        //    }
        //    else if (args.LeadingCount == 2)
        //    {
        //        return new Branch(gens[0], gens[1], Data.Terms.Boolean.False);
        //    }
        //    else
        //    {
        //        return new Branch(gens[0], gens[1], gens[2]);
        //    }
        //}

        //private static Sequence ParseSequence(FlatList<AstNode> args, Binding.BindingStore bs)
        //{
        //    if (args.Values.Last() is not Generative finalGen)
        //    {
        //        throw new ParserException(
        //            "The '{0}' form must conclude with a generative form, but was given: {1}",
        //            Symbol.Begin.Name,
        //            args.Values.Last());
        //    }
        //    else
        //    {
        //        return new Sequence(args.Values, finalGen);
        //    }
        //}

        //private static BindFixed ParseDefineFixed(FlatList<AstNode> args, Binding.BindingStore bs)
        //{
        //    throw new NotImplementedException();
        //}

        //private static BindSyntax ParseDefineSyntax(FlatList<AstNode> args, Binding.BindingStore bs)
        //{
        //    throw new NotImplementedException();
        //}

        //private static SetFixed ParseSetFixed(FlatList<AstNode> args, Binding.BindingStore bs)
        //{
        //    throw new NotImplementedException();
        //}

        //private static Appl ParseApplication(Generative op, AstNode tail, Binding.BindingStore bs)
        //{
        //    if (FlatList<Generative>.FromNested(tail) is FlatList<Generative> args
        //        && !args.IsDotted)
        //    {
        //        return new Appl(op, args.Values);
        //    }
        //    else
        //    {
        //        throw new ParserException("Given node cannot be tail of applicative form: {0}", tail);
        //    }
        //}
    }
}
