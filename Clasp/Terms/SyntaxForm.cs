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

    internal static class Env_Helper
    {
        public static Env MakeEmpty() => Array.Empty<EnvBinding>();
        public static Env MakeSolo(Symbol key, Expression def) => new Dictionary<Symbol, RecDef>()
        {
            { key, new RecDef(0, def) }
        };

        public static Dictionary<Symbol, RecDef> ToDictionary(this Env e) => e.ToDictionary(x => x.Key, x => x.Value);


        private static readonly EnvBinding _nullBinding = new EnvBinding(Symbol.Underscore, new RecDef(-1, Expression.Nil));

        public static bool TryLookup(this Env rho, Symbol key, out RecDef def)
        {
            if (rho.FirstOrDefault(x => x.Key == key, _nullBinding) is EnvBinding eb
                && !eb.Equals(_nullBinding))
            {
                def = eb.Value;
                return true;

            }
            def = _nullBinding.Value;
            return false;
        }

        public static RecDef Lookup(this Env rho, Symbol key)
        {
            if (TryLookup(rho, key, out RecDef def))
            {
                return def;
            }

            throw new Exception($"Key '{key}' isn't contained in Env {rho.Print()}");
        }

        public static IEnumerable<Env> Decompose(this Env rho)
        {
            if (rho.All(x => x.Value.Item2.IsNil)) //stopnow?
            {
                return Array.Empty<Env>();
            }
            else if (!rho.All(x => x.Value.Item2 is Pair)) //unequal lengths?
            {
                throw new Exception($"Variable list lengths in recurrent elements of {rho.Print()}");
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

        public SyntaxRule(SyntaxForm pat, SyntaxForm tem)
        {
            Pattern = pat;
            Template = tem;
        }

        public SyntaxRule(Expression pat, Expression tem)
        {
            Pattern = SyntaxForm.ParsePattern(pat);
            Template = SyntaxForm.ParseTemplate(tem);
        }

        public bool TryTransform(Expression input, Expression literalIDs, out Expression output)
        {
            if (Pattern.Beta(input))
            {
                Env mid = Pattern.Delta(input);
                output = Template.Tau(mid);
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
        public override Expression Car => throw new NotImplementedException();
        public override Expression Cdr => throw new NotImplementedException();
        public override Expression SetCar(Expression expr) => throw new NotImplementedException();
        public override Expression SetCdr(Expression expr) => throw new NotImplementedException();

        public override bool IsAtom => false;


        public abstract IEnumerable<Symbol> Identifiers { get; }

        public abstract bool Beta(Expression s);
        public abstract Env Delta(Expression s);
        public abstract Expression Tau(Env rho);


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
                    if (car is not SyntacticIdentifier sid || sid.Identifier != Symbol.Ellipsis)
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

        public override bool Beta(Expression s) => s.IsNil;
        public override Env Delta(Expression s) => Env_Helper.MakeEmpty();
        public override Expression Tau(Env rho) => Nil;
    }

    internal sealed class SyntacticIdentifier : SyntaxForm
    {
        public override IEnumerable<Symbol> Identifiers => new Symbol[] { Identifier };

        public readonly Symbol Identifier;

        public SyntacticIdentifier(Symbol sym) => Identifier = sym;

        public override string ToSerialized() => Identifier.ToSerialized();
        public override string ToPrinted() => Identifier.ToPrinted();

        public override bool Beta(Expression s) => true;
        public override Env Delta(Expression s) => Identifier == Symbol.Underscore ? Env_Helper.MakeEmpty() : Env_Helper.MakeSolo(Identifier, s);
        public override Expression Tau(Env rho)
        {
            if (rho.TryLookup(Identifier, out RecDef def))
            {
                if (def.Item1 == 0)
                {
                    return def.Item2;
                }
                else
                {
                    throw new Exception($"Tried to reference key '{Identifier}' with recurrence level {def.Item1} in {rho.Print()}");
                }
            }
            else
            {
                return Identifier;
            }
        }
    }

    internal sealed class SyntacticPair : SyntaxForm
    {
        public override IEnumerable<Symbol> Identifiers => _identifiers;

        private IEnumerable<Symbol> _identifiers;

        public readonly SyntaxForm Head;
        public readonly SyntaxForm Tail;

        public SyntacticPair(SyntaxForm hd, SyntaxForm tl)
        {
            Head = hd;
            Tail = tl;

            _identifiers = hd.Identifiers.Union(tl.Identifiers);
        }

        public override string ToSerialized() => $"{{{Head.ToSerialized()}{SerializeTail(Tail)}}}";
        public override string ToPrinted() => Pair.Cons(Head, Tail).ToPrinted();

        private static string SerializeTail(SyntaxForm sf)
        {
            return sf switch
            {
                SyntacticEmpty => string.Empty,
                SyntacticIdentifier sid => " " + sid.ToSerialized(),
                SyntacticPair sp => " " + sp.Head.ToSerialized() + SerializeTail(sp.Tail),
                SyntacticRepeating sr => " " + sr.ToSerialized(),
                SyntacticDatum sd => " . " + sd.ToSerialized(),
                _ => throw new Exception("Invalid Syntax")
            };
        }

        public override bool Beta(Expression s) => s is Pair p && Head.Beta(p.Car) && Tail.Beta(p.Cdr);
        public override Env Delta(Expression s) => Head.Delta(s.Car).Union(Tail.Delta(s.Car)); //check here for equivalent symbol bindings?
        public override Expression Tau(Env rho) => Head is SyntacticRepeating
            ? Pair.Append(Head.Tau(rho), Tail.Tau(rho))
            : Pair.Cons(Head.Tau(rho), Tail.Tau(rho));
    }

    internal sealed class SyntacticRepeating : SyntaxForm
    {
        public override IEnumerable<Symbol> Identifiers => RepeatingTerm.Identifiers;

        public readonly SyntaxForm RepeatingTerm;

        public SyntacticRepeating(SyntaxForm rep) => RepeatingTerm = rep;

        public override string ToSerialized() => RepeatingTerm.ToSerialized() + " ...";
        public override string ToPrinted() => RepeatingTerm.ToPrinted() + " ...";

        public override bool Beta(Expression s) => s.IsNil || (s is Pair p && RepeatingTerm.Beta(p.Car) && Beta(p.Cdr));
        public override Env Delta(Expression s)
        {
            var terms = Pair.Enumerate(s);
            var splitEnvs = terms.Last().IsNil
                ? terms.SkipLast(1).Select(x => RepeatingTerm.Delta(x))
                : terms.Select(x => RepeatingTerm.Delta(x));

            return Identifiers.Select(x => new EnvBinding(x, new RecDef(
                splitEnvs.First().Lookup(x).Item1 + 1,
                Pair.MakeList(splitEnvs.Select(y => y.Lookup(x).Item2).ToArray()))));
        }
        public override Expression Tau(Env rho)
        {
            var subset = rho.Where(x => Identifiers.Contains(x.Key));

            if (!subset.Any(x => x.Value.Item1 > 0)) //controllable?
            {
                throw new Exception($"No key of recurrence level > 0 in {rho.Print()}");
            }
            else
            {
                var pieces = subset.Decompose();
                IEnumerable<Expression> elements = pieces.Select(x => RepeatingTerm.Tau(x));

                return RepeatingTerm is SyntacticRepeating
                    ? elements.Aggregate(Nil as Expression, (x, y) => Pair.Append(x, y))
                    : Pair.MakeList(elements.ToArray());
            }

        }
    }

    internal sealed class SyntacticDatum : SyntaxForm
    {
        public override IEnumerable<Symbol> Identifiers => Array.Empty<Symbol>();

        public readonly Expression Datum;

        public SyntacticDatum(Expression expr) => Datum = expr;

        public override string ToSerialized() => Datum.ToSerialized();
        public override string ToPrinted() => ToSerialized();

        public override bool Beta(Expression s) => Datum.Equal_q(s);
        public override Env Delta(Expression s) => Env_Helper.MakeEmpty();
        public override Expression Tau(Env rho) => Datum;
    }

}
