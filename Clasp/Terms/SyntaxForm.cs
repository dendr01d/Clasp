using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    using Env = IEnumerable<KeyValuePair<Symbol, Tuple<int, Expression>>>;
    using EnvBinding = KeyValuePair<Symbol, Tuple<int, Expression>>;
    using RecDef = Tuple<int, Expression>;

    internal static class Env_Helpers
    {
        public static Env MakeEmpty() => Array.Empty<EnvBinding>();
        public static Env MakeSolo(Symbol key, Expression def) => new Dictionary<Symbol, RecDef>()
        {
            { key, new RecDef(0, def) }
        };

        public static Env MakeBlanks(IEnumerable<Symbol> ids) => ids.ToDictionary(x => x, x => new RecDef(0, Expression.Nil));

        public static Dictionary<Symbol, RecDef> ToDictionary(this Env e) => e.ToDictionary(x => x.Key, x => x.Value);

        public static IEnumerable<Symbol> Keys(this Env rho) => rho.Select(x => x.Key);

        private static readonly EnvBinding _nullBinding = new EnvBinding(Symbol.Underscore, new RecDef(-1, Expression.Nil));

        public static bool TryLookup(this Env rho, Symbol key, out RecDef def)
        {
            def = new RecDef(-1, Expression.Nil);

            if (rho.FirstOrDefault(x => x.Key == key, default(EnvBinding)) is EnvBinding eb
                && !eb.Equals(default(EnvBinding)))
            {
                def = eb.Value;
                return true;
            }

            return false;
        }

        public static RecDef Lookup(this Env rho, Symbol key)
        {
            if (TryLookup(rho, key, out RecDef def))
            {
                return def;
            }

            throw new UncategorizedException($"Key '{key}' isn't contained in Env {rho.Print()}");
        }

        public static bool TryMerge(this Env e1, Env e2, out Env result)
        {
            result = e1.Union(e2);

            foreach(Symbol collision in e1.Keys().Intersect(e2.Keys()))
            {
                if (e1.Lookup(collision) is RecDef def1
                    && e2.Lookup(collision) is RecDef def2)
                {
                    if (def1.Item1 != def2.Item1)
                    {
                        return false;
                    }
                    else if (!Expression.Pred_Equal(def1.Item2, def2.Item2))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static Env SubsetKeys(this Env rho, IEnumerable<Symbol> ids) => rho.Where(x => ids.Contains(x.Key));

        public static IEnumerable<Env> Decompose(this Env rho)
        {
            if (rho.All(x => x.Value.Item2.IsNil))
            {
                //stop recurring when all entries exhausted
                return Array.Empty<Env>();
            }
            else if (!rho.Any(x => x.Value.Item1 > 0))
            {
                //ensure there's at least one recurring element against which to gauge length of repeating element
                //AKA "controllable" check from Kohlbecker's paper
                throw new UncategorizedException($"No recurrent elements present in decomposing {rho.Print()}");

            }
            else if (!rho.Where(x => x.Value.Item1 > 0).All(x => x.Value.Item2 is Pair))
            {
                //ensure all recurring elements are of equal length
                //length isn't checked directly, but as the decomposition recurs
                throw new UncategorizedException($"Variable list lengths in recurrent elements of {rho.Print()}");
            }
            else
            {
                Env head = rho.Select(x => new EnvBinding(x.Key, Split(y => y.Car, x.Value)));
                Env rest = rho.Select(x => new EnvBinding(x.Key, new RecDef(x.Value.Item1, x.Value.Item2.Cdr)));

                return new Env[] { head }.Concat(Decompose(rest));
            }
        }

        private static RecDef Split(Func<Expression, Expression> fun, RecDef tup)
        {
            return tup.Item1 == 0
                ? tup
                : new RecDef(tup.Item1 - 1, fun(tup.Item2));
        }

        public static string Print(this Env rho)
        {
            StringBuilder sb = new StringBuilder().AppendLine("Env {");

            int keyWidth = rho.Max(x => x.Key.Name.Length);

            foreach(EnvBinding binding in rho)
            {
                sb.Append(string.Format("\t{0," + keyWidth.ToString() + "} --> ", binding.Key.Name));
                sb.AppendLine($"<{binding.Value.Item1}, {binding.Value.Item2.ToSerialized()}>");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }

    internal sealed class SyntaxRule : Expression
    {
        public readonly SyntaxForm Pattern;
        public readonly SyntaxForm Template;

        private readonly IEnumerable<Symbol> _freeIDentifiers;

        public SyntaxRule(SyntaxForm pat, SyntaxForm tem)
        {
            Pattern = pat;
            Template = tem;

            _freeIDentifiers = Template.Identifiers.Except(Pattern.Identifiers);
        }

        public SyntaxRule(Expression pat, Expression tem) : this(
            SyntaxForm.ParsePattern(pat),
            SyntaxForm.ParseTemplate(tem))
        { }

        public bool TryTransform(Expression input, Expression literalIDs, Environment macroEnv, Environment exprEnv, out Expression output)
        {
            Symbol[] literals = Pair.Enumerate(literalIDs).Select(x => x.Expect<Symbol>()).ToArray();

            if (Pattern.TryMatch(input, literals, out Env bindings))
            {
                Dictionary<Symbol, Symbol> alphaExchange = _freeIDentifiers
                    .ToDictionary(x => x, x => new GenSym(x.Name) as Symbol);

                foreach(var replacement in alphaExchange)
                {
                    if (macroEnv.HasBound(replacement.Key))
                    {
                        exprEnv.BindNew(replacement.Value, macroEnv.LookUp(replacement.Key));
                    }
                }

                output = Template.Build(bindings, alphaExchange);
                return true;
            }
            else
            {
                output = Nil;
                return false;
            }
        }

        public override Expression Car => throw new NotImplementedException();
        public override Expression Cdr => throw new NotImplementedException();
        public override Expression SetCar(Expression expr) => throw new NotImplementedException();
        public override Expression SetCdr(Expression expr) => throw new NotImplementedException();
        public override string ToSerialized() => Pair.MakeList(Pattern, Template).ToSerialized();
        public override string ToPrinted() => Pair.MakeList(Pattern, Template).ToPrinted();
        public override bool IsAtom => true;
    }

    internal abstract class SyntaxForm : Expression
    {
        public abstract IEnumerable<Symbol> Identifiers { get; }
        public abstract bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings);
        public abstract Expression Build(Env bindings, IDictionary<Symbol, Symbol> alphaConversion);

        public abstract override bool IsAtom { get; }
        public override Expression Car => throw new ExpectedTypeException<Pair>(this);
        public override Expression Cdr => throw new ExpectedTypeException<Pair>(this);
        public override Expression SetCar(Expression expr) => throw new ExpectedTypeException<Pair>(this);
        public override Expression SetCdr(Expression expr) => throw new ExpectedTypeException<Pair>(this);

        public static SyntaxForm ParsePattern(Expression expr)
        {
            if (expr.IsNil)
            {
                return new SyntacticEmpty();
            }
            else if (expr is Symbol sym)
            {
                return new SyntacticIdentifier(sym);
            }
            else if (expr is Pair p)
            {
                if (p.Cdr is Pair p2 && p2.Car == Symbol.Ellipsis && p2.Cdr.IsNil)
                {
                    return new SyntacticRepeating(ParsePattern(p.Car));
                }
                else
                {
                    return new SyntacticPair(
                        ParsePattern(p.Car),
                        ParsePattern(p.Cdr));
                }
            }
            else
            {
                return new SyntacticDatum(expr);
            }
        }

        public static SyntaxForm ParseTemplate(Expression expr)
        {
            if (expr.IsNil)
            {
                return new SyntacticEmpty();
            }
            else if (expr is Symbol sym)
            {
                return new SyntacticIdentifier(sym);
            }
            else if (expr is Pair p)
            {
                SyntaxForm car = ParseTemplate(p.Car);

                while (p.Cdr is Pair p2 && p2.Car == Symbol.Ellipsis)
                {
                    if (car is not SyntacticIdentifier sid || sid.Identifiers.First() != Symbol.Ellipsis)
                    {
                        car = new SyntacticRepeating(car);
                    }

                    p = p2;
                }

                SyntaxForm cdr = ParseTemplate(p.Cdr);
                return new SyntacticPair(car, cdr);
            }
            else
            {
                return new SyntacticDatum(expr);
            }
        }
    }

    internal sealed class SyntacticEmpty : SyntaxForm
    {
        public override IEnumerable<Symbol> Identifiers => Array.Empty<Symbol>();

        public override string ToSerialized() => "{}";
        public override string ToPrinted() => Nil.ToPrinted();
        public override bool IsAtom => true;

        public override bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings)
        {
            bindings = Env_Helpers.MakeEmpty();
            return s.IsNil;
        }
        public override Expression Build(Env bindings, IDictionary<Symbol, Symbol> alphaConversion) => Nil;
    }

    internal sealed class SyntacticIdentifier : SyntaxForm
    {
        public override IEnumerable<Symbol> Identifiers => new Symbol[] { _id };

        private readonly Symbol _id;
        public SyntacticIdentifier(Symbol sym) => _id = sym;

        public override string ToSerialized() => _id.ToSerialized();
        public override string ToPrinted() => _id.ToPrinted();
        public override bool IsAtom => true;

        public override bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings)
        {
            bindings = _id == Symbol.Underscore || literalIDs.Contains(_id)
                ? Env_Helpers.MakeEmpty()
                : Env_Helpers.MakeSolo(_id, s);
            return true;
        }
        public override Expression Build(Env bindings, IDictionary<Symbol, Symbol> alphaConversion)
        {
            if (alphaConversion.TryGetValue(_id, out Symbol? newId))
            {
                return newId;
            }
            else if (bindings.TryLookup(_id, out RecDef def))
            {
                return def.Item1 == 0
                    ? def.Item2
                    : throw new Exception($"Tried to reference {_id} with recurrence level {def.Item1} in {bindings.Print()}");
            }
            return _id;
        }
    }

    internal sealed class SyntacticPair : SyntaxForm
    {
        public override IEnumerable<Symbol> Identifiers => _ids;

        private IEnumerable<Symbol> _ids;

        private readonly SyntaxForm _head;
        private readonly SyntaxForm _tail;

        public override Expression Car => _head;
        public override Expression Cdr => _tail;

        public SyntacticPair(SyntaxForm hd, SyntaxForm tl)
        {
            _head = hd;
            _tail = tl;

            _ids = hd.Identifiers.Union(tl.Identifiers);
        }

        public override string ToSerialized() => $"{{{_head.ToSerialized()}{SerializeTail(_tail)}}}";
        public override string ToPrinted() => Pair.Cons(_head, _tail).ToPrinted();
        public override bool IsAtom => false;

        private static string SerializeTail(SyntaxForm sf)
        {
            return sf switch
            {
                SyntacticEmpty => string.Empty,
                SyntacticIdentifier sid => " " + sid.ToSerialized(),
                SyntacticPair sp => " " + sp._head.ToSerialized() + SerializeTail(sp._tail),
                SyntacticRepeating sr => " " + sr.ToSerialized(),
                SyntacticDatum sd => " . " + sd.ToSerialized(),
                _ => throw new Exception("Invalid Syntax")
            };
        }

        public override bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings)
        {
            if (s is Pair p
                && _head.TryMatch(p.Car, literalIDs, out Env headB)
                && _tail.TryMatch(p.Cdr, literalIDs, out Env tailB)
                && headB.TryMerge(tailB, out Env merged))
            {
                bindings = merged;
                return true;
            }
            else
            {
                bindings = Env_Helpers.MakeEmpty();
                return false;
            }
        }

        public override Expression Build(Env bindings, IDictionary<Symbol, Symbol> alphaConversion)
        {
            Expression first = _head.Build(bindings, alphaConversion);
            Expression rest = _tail.Build(bindings, alphaConversion);

            return _head is SyntacticRepeating
                ? Pair.Append(first, rest)
                : Pair.Cons(first, rest);
        }
    }

    internal sealed class SyntacticRepeating : SyntaxForm
    {
        public override IEnumerable<Symbol> Identifiers => RepeatingTerm.Identifiers;

        public readonly SyntaxForm RepeatingTerm;

        public SyntacticRepeating(SyntaxForm rep) => RepeatingTerm = rep;

        public override string ToSerialized() => RepeatingTerm.ToSerialized() + " ...";
        public override string ToPrinted() => RepeatingTerm.ToPrinted() + " ...";
        public override bool IsAtom => false;

        public override bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings)
        {
            if (s.IsNil)
            {
                bindings = Env_Helpers.MakeBlanks(Identifiers);
                return true;
            }
            else if (s is Pair p
                && RepeatingTerm.TryMatch(p.Car, literalIDs, out Env bindCar)
                && TryMatch(p.Cdr, literalIDs, out Env bindCdr))
            {
                bindings = bindCar.Select(x => new EnvBinding(
                    x.Key,
                    new RecDef(
                        x.Value.Item1 + 1,
                        Pair.Cons(x.Value.Item2, bindCdr.Lookup(x.Key).Item2))));
                return true;
            }

            bindings = Env_Helpers.MakeEmpty();
            return false;
        }

        public override Expression Build(Env bindings, IDictionary<Symbol, Symbol> alphaConversion)
        {
            Env subset = bindings.SubsetKeys(Identifiers);
            IEnumerable<Env> splitEnvs = subset.Decompose();

            IEnumerable<Expression> elements = splitEnvs.Select(x => RepeatingTerm.Build(x, alphaConversion));
            return RepeatingTerm is SyntacticRepeating
                ? elements.Aggregate(Nil as Expression, (x, y) => Pair.Append(x, y))
                : Pair.MakeList(elements.ToArray());
        }
    }

    internal sealed class SyntacticDatum : SyntaxForm
    {
        public override IEnumerable<Symbol> Identifiers => Array.Empty<Symbol>();

        public readonly Expression Datum;

        public SyntacticDatum(Expression expr) => Datum = expr;

        public override string ToSerialized() => Datum.ToSerialized();
        public override string ToPrinted() => ToSerialized();
        public override bool IsAtom => true;

        public override bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings)
        {
            bindings = Env_Helpers.MakeEmpty();
            return Expression.Pred_Equal(Datum, s);
        }

        public override Expression Build(Env bindings, IDictionary<Symbol, Symbol> alphaConversion) => Datum;
    }

}
