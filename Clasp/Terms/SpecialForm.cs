//namespace Clasp
//{
//    internal abstract class SpecialForm : Pair
//    {
//        protected abstract int Arity { get; }
//        protected abstract string OpName { get; }

//        protected SpecialForm(Symbol sym, Expression[] exprs) : base(sym, ConstructLinked(exprs))
//        {
//            if (exprs.Length < Arity)
//            {
//                throw new Exception($"Special form '{OpName}' requires (at least) {Arity} argument{(Arity == 1 ? string.Empty : "s")}");
//            }
//        }

//        private static readonly Dictionary<string, Func<Symbol, Expression[], SpecialForm>> _constructors = new()
//        {
//            ["if"] = (x, y) => new SPIf(x, y),
//            ["quote"] = (x, y) => new SPQuote(x, y),
//            ["define"] = (x, y) => new SPDefine(x, y),
//            ["car"] = (x, y) => new SPCar(x, y),
//            ["cdr"] = (x, y) => new SPCdr(x, y),
//            ["cons"] = (x, y) => new SPCons(x, y),
//            ["lambda"] = (x, y) => new SPLambda(x, y)
//        };

//        public static bool IsSpecialKeyword(Expression s)
//        {
//            return (s is Symbol sym && _constructors.ContainsKey(sym.Name));
//        }

//        public static SpecialForm CreateForm(Expression title, Expression[] exprs)
//        {
//            Symbol sym = title.ExpectSymbol();
//            if (_constructors.TryGetValue(sym.Name, out var constructor) && constructor is not null)
//            {
//                return constructor.Invoke(sym, exprs);
//            }
//            else
//            {
//                throw new Exception($"Tried to create special form with keyword '{sym}'");
//            }
//        }

//        public abstract override Expression Evaluate(Frame env);

//    }

//    internal class SPIf : SpecialForm
//    {
//        protected override int Arity => 3;
//        protected override string OpName => "if";
//        public SPIf(Symbol sym, Expression[] exprs) : base(sym, exprs) { }
//        public override Expression Evaluate(Frame env)
//        {
//            return ExpectArg(1).Evaluate(env).IsTrue
//                ? ExpectArg(2).Evaluate(env)
//                : ExpectArg(3).Evaluate(env);
//        }
//    }

//    internal class SPQuote : SpecialForm
//    {
//        protected override int Arity => 1;
//        protected override string OpName => "quote";
//        public SPQuote(Symbol sym, Expression[] exprs) : base(sym, exprs) { }
//        public override Expression Evaluate(Frame env)
//        {
//            return ExpectArg(1);
//        }
//    }

//    internal class SPDefine : SpecialForm
//    {
//        protected override int Arity => 2;
//        protected override string OpName => "define";
//        public SPDefine(Symbol sym, Expression[] exprs) : base(sym, exprs) { }
//        public override Expression Evaluate(Frame env)
//        {
//            if (ExpectArg(1).IsAtom)
//            {
//                env.Extend(ExpectArg(1).ExpectSymbol(), ExpectArg(2));
//            }
//            else
//            {
//                Symbol key = ExpectArg(1).ExpectCar().ExpectSymbol();
//                Pair parameters = ExpectArg(1).ExpectCdr().ExpectList();
//                Expression body = ExpectArg(2);
//                Procedure lambda = Procedure.BuildLambda(parameters, body, env);

//                env.Extend(key, lambda);
//            }

//            return TrueValue;
//        }
//    }

//    internal class SPCar : SpecialForm
//    {
//        protected override int Arity => 1;
//        protected override string OpName => "car";
//        public SPCar(Symbol sym, Expression[] exprs) : base(sym, exprs) { }
//        public override Expression Evaluate(Frame env)
//        {
//            return ExpectArg(1).Evaluate(env).ExpectCar();
//        }
//    }

//    internal class SPCdr : SpecialForm
//    {
//        protected override int Arity => 1;
//        protected override string OpName => "cdr";
//        public SPCdr(Symbol sym, Expression[] exprs) : base(sym, exprs) { }
//        public override Expression Evaluate(Frame env)
//        {
//            return ExpectArg(1).Evaluate(env).ExpectCdr();
//        }
//    }

//    internal class SPCons : SpecialForm
//    {
//        protected override int Arity => 2;
//        protected override string OpName => "cons";
//        public SPCons(Symbol sym, Expression[] exprs) : base(sym, exprs) { }
//        public override Expression Evaluate(Frame env)
//        {
//            return Pair.Cons(ExpectArg(1).Evaluate(env), ExpectArg(2).Evaluate(env));
//        }
//    }

//    internal class SPLambda : SpecialForm
//    {
//        protected override int Arity => 1;
//        protected override string OpName => "lambda";
//        public SPLambda(Symbol sym, Expression[] exprs) : base(sym, exprs) { }
//        public override Expression Evaluate(Frame env)
//        {
//            Pair parameters = ExpectArg(1).ExpectList();
//            Expression body = ExpectArg(2);
//            Procedure lambda = Procedure.BuildLambda(parameters, body, env);

//            return lambda;
//        }
//    }
//}
