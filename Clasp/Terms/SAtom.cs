namespace Clasp
{
    internal abstract class SAtom : Expression
    {
        protected SAtom() { }

        public override bool IsAtom => true;
        public override bool IsList => false;

        public override Expression Evaluate(Environment env) => this;

    }

    internal class Symbol : SAtom
    {
        public readonly string Name;
        public Symbol(string name) => Name = name;

        public override bool EqualsByValue(Expression? other) => other is Symbol sym && sym.Name == Name;

        public override Expression Evaluate(Environment env) => env.Lookup(this);
        public override string ToString() => Name;
    }

    internal class Procedure : SAtom
    {
        public readonly string Name;
        private readonly Func<SList, Environment, Expression> _operation;

        #region construction

        public Procedure(string name, Func<SList, Environment, Expression> op)
        {
            Name = name;
            _operation = op;
        }

        public Procedure(string name, Func<SList, Expression> op) : this(name, (l, _) => op.Invoke(l)) { }
        public Procedure(string name, Action<SList> op) : this(name, Funcify(op)) { }
        public Procedure(string name, Action<SList, Environment> op) : this(name, Funcify(op)) { }

        private static Func<SList, Expression> Funcify(Action<SList> action)
        {
            return l => { action.Invoke(l); return TrueValue; };
        }
        private static Func<SList, Environment, Expression> Funcify(Action<SList, Environment> action)
        {
            return (l, a) => { action.Invoke(l, a); return TrueValue; };
        }

        #endregion

        public Expression Apply(SList args, Environment env)
        {
            return _operation(args, env);
        }

        public Expression Apply(Expression arg, Environment env)
        {
            return arg.IsAtom
                ? Apply(Pair.Cons(arg, Nil), env)
                : Apply(arg.AsList(), env);
        }

        public static Procedure BuildLambda(Symbol[] parameters, Expression body, Environment closure)
        {
            string name = $"lambda ({string.Join(' ', parameters.AsEnumerable())}) {body}";

            return new Procedure(name, (SList l, Environment _) =>
            {
                Environment env = new(closure);
                for (int i = 0; i < parameters.Length; ++i)
                {
                    //Expression def = i + 1 < parameters.Length
                    //    ? l.AtIndex(i)
                    //    : l.FromIndex(i); //for the last parameter, grab everything left
                    env.Extend(parameters[i], l.AtIndex(i));
                }
                return body.Evaluate(env);
            });
        }

        public override string ToString() => $"<{Name}>";

        public override bool EqualsByValue(Expression? other)
        {
            return other is Procedure p
                && p.Name == Name
                && p._operation == _operation;
        }
    }

    //internal abstract class Constant : SAtom
    //{

    //}

    internal abstract class Literal<T> : SAtom
        where T : struct
    {
        public readonly T Value;
        protected Literal(T val)
        {
            Value = val;
        }
        public override bool EqualsByValue(Expression? other) => other is Literal<T> lt && lt.Value.Equals(Value);
    }

    internal class Number : Literal<double>
    {
        public Number(double d) : base(d) { }

        public override string ToString() => Value.ToString();
    }

    internal class Character : Literal<char>
    {
        public Character(char c) : base(c) { }
        public override string ToString() => $"\\{Value}";
    }

    //internal abstract class SVector<T> : Constant
    //{
    //    public readonly Literal<T> Contents;
    //}

    //internal abstract class SString : SVector<Character>
    //{
    //    //I don't think the contents here are typed correctly??
    //}
}
