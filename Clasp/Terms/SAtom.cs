namespace Clasp
{
    internal abstract record SAtom() : Expression()
    {
        public override bool IsAtom => true;
        public override bool IsList => false;
        protected override Recurrence Evaluate(Environment env) => FinishedEval(this);

    }

    internal sealed record Symbol(string Name) : SAtom()
    {
        protected override Recurrence Evaluate(Environment env) => FinishedEval(env.Lookup(this));
        protected override string FormatString() => Name;
    }

    internal abstract record Operator(string Name) : SAtom()
    {
        public abstract Recurrence Apply(SList args, Environment env);
        protected override string FormatString() => $"<{Name}>";
        public static Recurrence StdApply(Operator op, Expression expr, Environment env)
        {
            return op.Apply(expr.The<SList>(), env);
        }
    }

    internal sealed record Procedure(string Name, Func<SList, Environment, Expression> Proc) : Operator(Name)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            return FinishedEval(Proc(args, env));
        }
    }

    internal sealed record Lambda(Symbol[] Parameters, Expression Body, Environment Closure) : Operator(FormatLambda(Parameters, Body))
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            if (args.IsNil || Parameters.Length == 0)
            {
                return ContinueWith(Body, env, StdEval);
            }
            else
            {
                Environment local = new Environment(env);
                for (int i = 0; i < Parameters.Length; ++i)
                {
                    local.Extend(Parameters[i], args[i]);
                }
                return ContinueWith(Body, local, StdEval);
            }
        }

        private static string FormatLambda(Symbol[] parameters, Expression body)
        {
            return $"lambda ({string.Join(' ', parameters.AsEnumerable())}) {body}";
        }
    }

    //internal abstract class Constant : SAtom
    //{

    //}

    internal abstract record Literal<T>(T Value) : SAtom() where T : struct { }

    internal sealed record Number(double d) : Literal<double>(d)
    {
        protected override string FormatString() => Value.ToString();
    }

    internal sealed record Character(char c) : Literal<char>(c)
    {
        protected override string FormatString() => $"\\{Value}";
    }

    internal sealed record Boolean(bool b) : Literal<bool>(b)
    {
        protected override string FormatString() => Value ? TrueValue.Name : FalseValue.Name;
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
