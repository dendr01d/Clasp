namespace Clasp
{
    internal abstract class SpecialForm : SList
    {
        protected abstract int Arity { get; }
        protected abstract string OpName { get; }

        protected SpecialForm(Symbol sym, Expression args) : base(sym, args)
        {
            if (args.IsAtom && Arity > 1 || args.AsList().Select().Count() < Arity)
            {
                throw new ArgumentArityException(OpName, Arity, args.AsList().Select().Count());
            }
        }

        private static readonly Dictionary<string, Func<Symbol, Expression, SpecialForm>> _constructors = new()
        {
            ["if"] = (x, y) => new SPIf(x, y),
            ["cond"] = (x, y) => new SPCond(x, y),
            ["case"] = (x, y) => new SPCase(x, y),
            ["quote"] = (x, y) => new SPQuote(x, y),
            ["define"] = (x, y) => new SPDefine(x, y),
            ["set!"] = (x, y) => new SPSet(x, y),
            ["eq?"] = (x, y) => new SPEq(x, y),
            ["car"] = (x, y) => new SPCar(x, y),
            ["cdr"] = (x, y) => new SPCdr(x, y),
            ["cons"] = (x, y) => new SPCons(x, y),
            ["lambda"] = (x, y) => new SPLambda(x, y),
            ["let"] = (x, y) => new SPLet(x, y)
        };

        public static bool IsSpecialKeyword(Expression s)
        {
            return (s is Symbol sym && _constructors.ContainsKey(sym.Name));
        }

        public static SpecialForm CreateForm(Symbol form, Expression args)
        {
            if (_constructors.TryGetValue(form.Name, out var constructor) && constructor is not null)
            {
                return constructor.Invoke(form, args);
            }
            else
            {
                throw new SpecialFormKeywordNotFoundException(form.Name);
            }
        }

        public abstract override Expression Evaluate(Environment env);

    }

    internal class SPIf : SpecialForm
    {
        protected override int Arity => 3;
        protected override string OpName => "if";
        public SPIf(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            return AtIndex(1).Evaluate(env).IsTrue
                ? AtIndex(2).Evaluate(env)
                : AtIndex(3).Evaluate(env);
        }
    }

    internal class SPCond : SpecialForm
    {
        protected override int Arity => 1;
        protected override string OpName => "cond";
        public SPCond(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            int i = 1;
            SList target = AtIndex(i).AsList();

            while (!target.IsNil)
            {
                if (target.AtIndex(0).Evaluate(env).IsTrue)
                {
                    return target.AtIndex(1).Evaluate(env);
                }
                target = this.AtIndex(++i).AsList();
            }

            throw new ControlFalloutException(OpName);
        }
    }

    internal class SPCase : SpecialForm
    {
        protected override int Arity => 2;
        protected override string OpName => "case";
        public SPCase(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            Expression test = AtIndex(1).Evaluate(env);

            int i = 2;
            SList target = AtIndex(i).AsList();

            while (!target.IsNil)
            {
                bool atomMatch = target.Car().IsAtom && target.Car().EqualsByValue(test);
                bool listMatch = target.Car().AsList().Select().Any(x => x.EqualsByValue(test));

                if (atomMatch || listMatch)
                {
                    return target.AtIndex(1).Evaluate(env);
                }
                target = this.AtIndex(++i).AsList();
            }

            throw new ControlFalloutException(OpName);
        }
    }

    internal class SPQuote : SpecialForm
    {
        protected override int Arity => 1;
        protected override string OpName => "quote";
        public SPQuote(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            return AtIndex(1);
        }
    }

    internal class SPDefine : SpecialForm
    {
        protected override int Arity => 2;
        protected override string OpName => "define";
        public SPDefine(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            if (AtIndex(1).IsAtom)
            {
                env.Extend(AtIndex(1).AsSymbol(), AtIndex(2));
            }
            else
            {
                Symbol key = AtIndex(1).Car().AsSymbol();
                Symbol[] parameters = AtIndex(1).AsList().FromIndex(1).AsList().Select(x => x.AsSymbol()).ToArray();
                Expression body = AtIndex(2);
                Procedure lambda = Procedure.BuildLambda(parameters, body, env);

                env.Extend(key, lambda);
            }

            return TrueValue;
        }
    }

    internal class SPSet : SpecialForm
    {
        protected override int Arity => 2;
        protected override string OpName => "set!";
        public SPSet(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            env.Set(AtIndex(1).AsSymbol(), AtIndex(2));

            return TrueValue;
        }
    }

    internal class SPEq : SpecialForm
    {
        protected override int Arity => 2;
        protected override string OpName => "eq";
        public SPEq(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            return ReferenceEquals(AtIndex(1).Evaluate(env), AtIndex(2).Evaluate(env));
        }
    }

    internal class SPCar : SpecialForm
    {
        protected override int Arity => 1;
        protected override string OpName => "car";
        public SPCar(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            return AtIndex(1).Evaluate(env).Car();
        }
    }

    internal class SPCdr : SpecialForm
    {
        protected override int Arity => 1;
        protected override string OpName => "cdr";
        public SPCdr(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            return AtIndex(1).Evaluate(env).Cdr();
        }
    }

    internal class SPCons : SpecialForm
    {
        protected override int Arity => 2;
        protected override string OpName => "cons";
        public SPCons(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            return Pair.Cons(AtIndex(1).Evaluate(env), AtIndex(2).Evaluate(env));
        }
    }

    internal class SPLambda : SpecialForm
    {
        protected override int Arity => 1;
        protected override string OpName => "lambda";
        public SPLambda(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            Symbol[] parameters = AtIndex(1).IsList
                ? AtIndex(1).AsList().Select(x => x.AsSymbol()).ToArray()
                : [AtIndex(1).AsSymbol()];
            Expression body = AtIndex(2);
            Procedure lambda = Procedure.BuildLambda(parameters, body, env);

            return lambda;
        }
    }

    internal class SPLet : SpecialForm
    {
        protected override int Arity => 2;
        protected override string OpName => "let";
        public SPLet(Symbol sym, Expression args) : base(sym, args) { }
        public override Expression Evaluate(Environment env)
        {
            //we COULD reformat it as a lambda closure, but since we're immediately calling it, why bother?

            Environment closure = new(env);
            foreach(SList binding in AtIndex(1).AsList().Select(x => x.AsList()))
            {
                closure.Extend(binding.AtIndex(0).AsSymbol(), binding.AtIndex(1).Evaluate(env));
            }

            return AtIndex(2).Evaluate(closure);
        }
    }
}
