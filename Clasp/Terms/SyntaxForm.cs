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

    internal interface ISyntax
    {
        public IEnumerable<Symbol> Identifiers { get; }
        public bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings);
        public Expression Build(Env bindings);
    }

    //internal abstract class SyntaxForm : Expression
    //{
    //    public override Expression Car => throw new NotImplementedException();
    //    public override Expression Cdr => throw new NotImplementedException();
    //    public override Expression SetCar(Expression expr) => throw new NotImplementedException();
    //    public override Expression SetCdr(Expression expr) => throw new NotImplementedException();

    //    public override bool IsAtom => false;


    //    public abstract IEnumerable<Symbol> Identifiers { get; }

    //    public abstract bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings);
    //    public abstract Expression Build(Env bindings);


    //    public static SyntaxForm ParsePattern(Expression expr)
    //    {
    //        if (expr.IsNil)
    //        {
    //            return new SyntacticEmpty();
    //        }
    //        else if (expr is Symbol sym)
    //        {
    //            return new SyntacticIdentifier(sym);
    //        }
    //        else if (expr is Pair p)
    //        {
    //            if (p.Cdr is Pair p2 && p2.Car == Symbol.Ellipsis && p2.Cdr.IsNil)
    //            {
    //                return new SyntacticRepeating(ParsePattern(p.Car));
    //            }
    //            else
    //            {
    //                return new SyntacticPair(
    //                    ParsePattern(p.Car),
    //                    ParsePattern(p.Cdr));
    //            }
    //        }
    //        else
    //        {
    //            return new SyntacticDatum(expr);
    //        }
    //    }
    //    public static SyntaxForm ParseTemplate(Expression expr)
    //    {
    //        if (expr.IsNil)
    //        {
    //            return new SyntacticEmpty();
    //        }
    //        else if (expr is Symbol sym)
    //        {
    //            return new SyntacticIdentifier(sym);
    //        }
    //        else if (expr is Pair p)
    //        {
    //            SyntaxForm car = ParseTemplate(p.Car);

    //            while (p.Cdr is Pair p2 && p2.Car == Symbol.Ellipsis)
    //            {
    //                if (car is not SyntacticIdentifier sid || sid.Identifier != Symbol.Ellipsis)
    //                {
    //                    car = new SyntacticRepeating(car);
    //                }

    //                p = p2;
    //            }

    //            SyntaxForm cdr = ParseTemplate(p.Cdr);
    //            return new SyntacticPair(car, cdr);
    //        }
    //        else
    //        {
    //            return new SyntacticDatum(expr);
    //        }
    //    }
    //}


    internal sealed class SyntacticEmpty : Atom, ISyntax
    {
        public IEnumerable<Symbol> Identifiers => Array.Empty<Symbol>();



        public override string ToSerialized() => "{}";
        public override string ToPrinted() => Nil.ToPrinted();

        public bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings)
        {
            bindings = Env_Helpers.MakeEmpty();
            return s.IsNil;
        }
        public Expression Build(Env bindings) => Nil;
    }

    internal sealed class SyntacticIdentifier : Atom, ISyntax
    {
        public IEnumerable<Symbol> Identifiers => new Symbol[] { _id };

        private readonly Symbol _id;

        public SyntacticIdentifier(Symbol sym) => _id = sym;

        public override string ToSerialized() => _id.ToSerialized();
        public override string ToPrinted() => _id.ToPrinted();


        public bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings)
        {
            bindings = _id == Symbol.Underscore || literalIDs.Contains(_id)
                ? Env_Helpers.MakeEmpty()
                : Env_Helpers.MakeSolo(_id, s);
            return true;
        }
        public Expression Build(Env bindings)
        {
            if (bindings.TryLookup(_id, out RecDef def))
            {
                return def.Item1 == 0
                    ? def.Item2
                    : throw new Exception($"Tried to reference {_id} with recurrence level {def.Item1} in {bindings.Print()}");
            }
            return _id;
        }
    }

    internal sealed class SyntacticPair : Pair, ISyntax
    {
        public IEnumerable<Symbol> Identifiers => _ids;

        private IEnumerable<Symbol> _ids;

        public readonly ISyntax Head;
        public readonly ISyntax Tail;

        public SyntacticPair(ISyntax hd, ISyntax tl) : base(hd, tl)
        {
            Head = hd;
            Tail = tl;

            _ids = hd.Identifiers.Union(tl.Identifiers);
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

        public bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings)
        {
            if (s is Pair p
                && Head.TryMatch(p.Car, literalIDs, out Env headB)
                && Tail.TryMatch(p.Cdr, literalIDs, out Env tailB)
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
    }

    internal sealed class SyntacticRepeating : Atom, ISyntax
    {
        public IEnumerable<Symbol> Identifiers => RepeatingTerm.Identifiers;

        public readonly ISyntax RepeatingTerm;

        public SyntacticRepeating(ISyntax rep) => RepeatingTerm = rep;

        public override string ToSerialized() => RepeatingTerm.ToSerialized() + " ...";
        public override string ToPrinted() => RepeatingTerm.ToPrinted() + " ...";

        public bool TryMatch(Expression s, Symbol[] literalIDs, out Env bindings)
        {
            if (s.IsNil)
            {
                bindings = Env_Helpers.MakeBlank(Identifiers);
                return true;
            }
            else if (s is Pair p
                && R)
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
        public override Env Delta(Expression s) => Env_Helpers.MakeEmpty();
        public override Expression Tau(Env rho) => Datum;

        public override LvlEnv DeltaPrime_1(Expression s) => Array.Empty<LvlBinding>();
        public override FunEnv DeltaPrime_2(Expression s) => Array.Empty<FunBinding>();
        public override Func<Expression, Expression> TauPrime(EnvPrime rho) => s => Datum;
    }

}
