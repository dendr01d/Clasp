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
        public override string ToString() => Name;
    }

    internal class Procedure : SAtom
    {
        public readonly string Name;
        private readonly Func<SList, Environment, Expression> _operation;

        private Procedure(string name, Func<SList, Environment, Expression> op)
        {
            Name = name;
            _operation = op;
        }

        public Expression Apply(SList args, Environment env)
        {
            return _operation(args, env);
        }

        public static Procedure BuildLambda(SList parameters, Expression body, Environment closure)
        {
            string name = $"lambda {parameters} {body}";

            return new Procedure(name, (SList l, Environment _) =>
            {
                Environment env = new(closure);
                env.ExtendMany(parameters, l);
                return body.Evaluate(env);
            });
        }

        public override string ToString() => $"<{Name}>";
    }

    //internal abstract class Constant : SAtom
    //{

    //}

    internal abstract class Literal<T> : SAtom
    {
        public readonly T Value;
        protected Literal(T val)
        {
            Value = val;
        }
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
