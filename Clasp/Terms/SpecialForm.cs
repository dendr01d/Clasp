namespace Clasp
{
    internal abstract record SpecialForm(string Name, int Arity) : Operator(Name)
    {
        public static readonly SpecialForm[] Forms = new SpecialForm[]
        {
            new SPBegin(),
            new SPIf(), new SPCond(), //new SPCase(),
            new SPQuote(),
            new SPDefine(), new SPSet(), new SPLet(),
            new SPEq(),
            new SPCar(), new SPCdr(), new SPCons(),
            new SPLambda(),
            new SPAnd(), new SPOr()
            
        };
    }

    internal sealed record SPBegin() : SpecialForm("begin", 1)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            Expression current = args[0].CallEval(env);

            if (args.Cdr.IsNil)
            {
                return FinishedEval(current);
            }
            else
            {
                return StdApply(this, args.Cdr, env);
            }
        }
    }

    internal sealed record SPIf() : SpecialForm("if", 3)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            bool condition = args[0].CallEval(env).IsTrue;

            if (condition)
            {
                return ContinueWith(args[1], env, StdEval);
            }
            else
            {
                return ContinueWith(args[2], env, StdEval);
            }
        }
    }

    internal sealed record SPCond() : SpecialForm("cond", 1)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            if (args.IsNil)
            {
                throw new ControlFalloutException(Name);
            }

            bool condition = args[0].The<SList>()[0].CallEval(env).IsTrue;

            if (condition)
            {
                return ContinueWith(args[0].The<SList>()[1], env, StdEval);
            }
            else
            {
                return StdApply(this, args.Cdr, env);
            }
        }
    }

    //internal sealed record SPCase() : SpecialForm("case", 2)
    //{
    //    public override Recurrence Apply(SList args, Environment env)
    //    {
    //        Expression example = args[0].CallEval(env);

    //        return ContinueWith(args.Cdr, env, CheckCases);
    //    }

    //    private static Recurrence CheckCases(Expression example, Environment env)
    //    {
    //        if (example.IsNil)
    //        {
    //            throw new ControlFalloutException("case");
    //        }
    //        else if (example.The<SList>()[0].The<SList>()[0].The<SList>().Contents().Contains(example))
    //        {
    //            return StdEval(example.The<SList>()[0].The<SList>()[1], env);
    //        }
    //        else
    //        {
    //            return ContinueWith(example.The<SList>().Cdr, env, CheckCases);
    //        }
    //    }
    //}

    internal sealed record SPQuote() : SpecialForm("quote", 1)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            return FinishedEval(args[0]);
        }
    }

    internal sealed record SPQuasiQuote() : SpecialForm("quasiquote", 1)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            return FinishedEval(new Pair(new SPQuote(), args[0].The<SList>().QEvList(env)));
        }
    }

    internal sealed record SPUnQuote() : SpecialForm("unquote", 1)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            return StdEval(args[0], env);
        }
    }

    internal sealed record SPDefine() : SpecialForm("define", 2)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            if (args[0].IsAtom)
            {
                env.Extend(args[0].The<Symbol>(), args[1].CallEval(env));
            }
            else
            {
                IEnumerable<Symbol> syms = args[0].The<SList>().Contents().Select(x => x.The<Symbol>()).ToArray();
                Lambda lam = new Lambda(syms.Skip(1).ToArray(), args[1], env);

                env.Extend(syms.First(), lam);
            }

            return FinishedEval(TrueValue);
        }
    }

    internal sealed record SPSet() : SpecialForm("set!", 2)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            env.Set(args[0].The<Symbol>(), args[1]);
            return FinishedEval(TrueValue);
        }
    }

    internal sealed record SPLet() : SpecialForm("let", 2)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            Environment local = new(env);
            foreach (SList binding in args[0].The<SList>().Contents().Select(x => x.The<SList>()))
            {
                local.Extend(binding[0].The<Symbol>(), binding[1].CallEval(local));
            }

            return StdApply(new SPBegin(), args.Cdr, local);
        }
    }

    internal sealed record SPEq() : SpecialForm("eq?", 2)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            return FinishedEval(ReferenceEquals(args[0].CallEval(env), args[1].CallEval(env)));
        }
    }

    internal sealed record SPCar() : SpecialForm("car", 1)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            return FinishedEval(args[0].CallEval(env).GetCar());
        }
    }

    internal sealed record SPCdr() : SpecialForm("cdr", 1)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            return FinishedEval(args[0].CallEval(env).GetCdr());
        }
    }

    internal sealed record SPCons() : SpecialForm("cons", 2)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            Expression car = args[0].CallEval(env);
            Expression cdr = args[1].CallEval(env);

            if (car.IsNil && cdr.IsNil)
            {
                return FinishedEval(Nil);
            }
            else
            {
                return FinishedEval(new Pair(car, cdr));
            }
        }
    }

    internal sealed record SPLambda() : SpecialForm("lambda", 2)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            IEnumerable<Symbol> syms = args[0].The<SList>().Contents().Select(x => x.The<Symbol>()).ToArray();
            Lambda lam = new Lambda(syms.ToArray(), args[1], env);

            return FinishedEval(lam);
        }
    }

    internal sealed record SPAnd() : SpecialForm("and", 2)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            if (args.IsNil)
            {
                return FinishedEval(TrueValue);
            }
            else if (args[0].CallEval(env).IsFalse)
            {
                return FinishedEval(FalseValue);
            }
            else
            {
                return StdApply(this, args.Cdr, env);
            }
        }
    }

    internal sealed record SPOr() : SpecialForm("or", 2)
    {
        public override Recurrence Apply(SList args, Environment env)
        {
            if (args.IsNil)
            {
                return FinishedEval(FalseValue);
            }
            else if (args[0].CallEval(env).IsTrue)
            {
                return FinishedEval(TrueValue);
            }
            else
            {
                return StdApply(this, args.Cdr, env);
            }
        }
    }
}
