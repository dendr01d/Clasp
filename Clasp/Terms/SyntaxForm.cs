using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal abstract class SyntaxForm : Expression
    {
        public override bool IsAtom => false;
        public override Expression Car => throw new InvalidOperationException();
        public override Expression Cdr => throw new InvalidOperationException();
        public override Expression SetCar(Expression expr) => throw new InvalidOperationException();
        public override Expression SetCdr(Expression expr) => throw new InvalidOperationException();

        public abstract IEnumerable<Symbol> Identifiers { get; }

        protected static IEnumerable<Symbol> GetIdentifiers(Expression expr)
        {
            if (expr is SyntaxForm sf)
            {
                return sf.Identifiers;
            }
            else
            {
                List<Symbol> output = new List<Symbol>();
                AccumulateIdentifiers(expr, output);
                return output;
            }
        }

        private static void AccumulateIdentifiers(Expression expr, List<Symbol> destination)
        {
            if (expr is Symbol sym)
            {
                destination.Add(sym);
            }
            else if (expr is RepeatingTerm rt)
            {
                AccumulateIdentifiers(rt.Term, destination);
            }
            else if (expr is Pair p)
            {
                AccumulateIdentifiers(p.Car, destination);
                AccumulateIdentifiers(p.Cdr, destination);
            }
        }
    }

    internal sealed class RepeatingTerm : SyntaxForm
    {
        private IEnumerable<Symbol> _identifiers;
        public readonly Expression Term;
        public override IEnumerable<Symbol> Identifiers => _identifiers;

        public RepeatingTerm(Expression term)
        {
            Term = term;
            _identifiers = GetIdentifiers(term);
        }

        public override string ToSerialized() => Term.ToSerialized() + " ...";

        public override string ToPrinted() => $"{{{Term.ToPrinted()} ...}}";
    }

    internal sealed class SyntaxPattern : SyntaxForm
    {
        private readonly List<Symbol> _identifiers;
        public override IEnumerable<Symbol> Identifiers => _identifiers;

        private Expression _pattern;

        public SyntaxPattern(Expression pattern)
        {
            _identifiers = new List<Symbol>();
            _pattern = ParsePattern(pattern, _identifiers);
        }

        public override string ToSerialized() => _pattern.ToSerialized();
        public override string ToPrinted() => ToSerialized();

        #region bullshit

        private static Expression ParsePattern(Expression input, List<Symbol> ids)
        {
            if (input is Symbol sym)
            {
                ids.Add(sym);
                return sym;
            }
            else if (input is Pair p)
            {
                Expression car = ParsePattern(p.Car, ids);

                if (p.Cdr is Pair p2 && p2.Car == Symbol.Ellipsis && p2.Cdr.IsNil)
                {
                    return new RepeatingTerm(car);
                }
                else
                {
                    Expression cdr = ParsePattern(p.Cdr, ids);
                    return Pair.Cons(car, cdr);
                }
            }
            else
            {
                return input;
            }
        }

        public bool TryUnify(Expression input, Expression literalIDs, Environment mutEnv)
        {
            return literalIDs is Pair p
                && TryUnify(input, _pattern, Pair.Enumerate(p).Select(x => x.Expect<Symbol>()), mutEnv);
        }

        private static bool TryUnify(Expression input, Expression pattern, IEnumerable<Symbol> literalIDs, Environment mutEnv)
        {
            if (pattern is Symbol sym)
            {
                if (sym == Symbol.Underscore)
                {
                    return true;
                }
                else if (literalIDs.Contains(sym))
                {
                    return input == sym;
                }
                else if (mutEnv.HasLocal(sym))
                {
                    return Pred_Equal(mutEnv.LookUp(sym), input);
                }
                else
                {
                    mutEnv.BindNew(sym, input);
                    return true;
                }
            }
            else if (pattern is RepeatingTerm rep)
            {
                Environment listEnv = mutEnv.Close();

                foreach(Symbol pVar in GetIdentifiers(rep))
                {
                    listEnv.BindNew(pVar, Expression.Nil); //initialize all pattern variables to "empty" lists
                }

                while (input is Pair p) //try to incorporate each term of the sequence
                {
                    Environment subEnv = listEnv.Close();

                    if (TryUnify(p.Car, rep.Term, literalIDs, subEnv))
                    {
                        listEnv.SubsumeAndAppend(subEnv);
                    }
                    else
                    {
                        return false;
                    }

                    input = p.Cdr; //iterate
                }

                if (!input.IsNil) //ellipses can't be applied to improper lists
                {
                    return false;
                }
                else
                {
                    mutEnv.SubsumeAndAppend(listEnv);
                    return true;
                }
            }
            else if (pattern is Pair patPair)
            {
                return input is Pair iptPair
                    && TryUnify(iptPair.Car, patPair.Car, literalIDs, mutEnv)
                    && TryUnify(iptPair.Cdr, patPair.Cdr, literalIDs, mutEnv);
            }
            else
            {
                return Pred_Equal(input, pattern);
            }
        }

        #endregion
    }

    internal sealed class SyntaxTemplate : SyntaxForm
    {
        private readonly List<Symbol> _identifiers;
        public override IEnumerable<Symbol> Identifiers => _identifiers;

        private Expression _template;

        public SyntaxTemplate(Expression pattern)
        {
            _identifiers = new List<Symbol>();
            _template = ParseTemplate(pattern, _identifiers);
        }

        public override string ToSerialized() => _template.ToSerialized();
        public override string ToPrinted() => ToSerialized();

        #region bullshit

        private static Expression ParseTemplate(Expression input, List<Symbol> ids)
        {
            if (input is Symbol sym)
            {
                ids.Add(sym);
                return sym;
            }
            else if (input is Pair p)
            {
                Expression car = ParseTemplate(p.Car, ids);

                while (p.Cdr is Pair p2 && p2.Car == Symbol.Ellipsis)
                {
                    car = new RepeatingTerm(car);
                    p = p2;
                }

                Expression cdr = ParseTemplate(p.Cdr, ids);

                return Pair.Cons(car, cdr);
            }
            else
            {
                return input;
            }
        }

        public bool TryExpand(IEnumerable<Symbol> patternVars, Environment context, out Expression result)
        {
            return TryExpand(_template, patternVars, context, out result);
        }

        private static bool TryExpand(Expression template, IEnumerable<Symbol> patternVars, Environment context, out Expression result)
        {
            if (template is Symbol sym)
            {
                if (!patternVars.Contains(sym)) //literal symbol
                {
                    result = sym;
                    return true;
                }
                else if (!context.HasBound(sym)) //unbound pattern var
                {
                    result = Nil;
                    return false;
                }

                result = context.LookUp(sym); //bound in pattern match
                return true;
            }
            else if (template is RepeatingTerm rep)
            {
                List<Expression> seq = new List<Expression>();

                IEnumerable<Symbol> templateVars = GetIdentifiers(rep).Intersect(patternVars);

                Environment listEnv = context.DescendAndCopy(templateVars);

                while (!listEnv.AllKeysExhausted(templateVars))
                {
                    if (listEnv.TryBumpBindings(templateVars, out Environment subEnv)
                        && TryExpand(rep.Term, patternVars, subEnv, out Expression next))
                    {
                        seq.Add(next);
                    }
                    else
                    {
                        result = Nil;
                        return false;
                    }
                }

                result = Pair.MakeList(seq.ToArray());
                return true;
            }
            else if (template is Pair tempPair)
            {
                if (TryExpand(tempPair.Car, patternVars, context, out Expression car)
                    && TryExpand(tempPair.Cdr, patternVars, context, out Expression cdr))
                {
                    result = tempPair.Car is RepeatingTerm
                        ? Pair.Append(car, cdr)
                        : Pair.Cons(car, cdr);
                }

                result = Nil;
                return false;
            }

            result = template;
            return true;
        }

        #endregion
    }
}
