using System;

namespace Clasp
{
    internal class Environment
    {
        private readonly Dictionary<string, Expression> _bindings;
        private readonly Environment? _lexicalContext;
        public readonly StreamWriter? OutputStream;

        public Environment()
        {
            _bindings = [];
            _lexicalContext = null;
            OutputStream = null;
        }

        public Environment(StreamWriter? streamOut) : this()
        {
            OutputStream = streamOut;
        }

        public Environment(Environment context) : this()
        {
            _lexicalContext = context;
            OutputStream = context.OutputStream;
        }

        #region Extension & Lookup

        public void Extend(Symbol sym, Expression definition)
        {
            if (_bindings.TryGetValue(sym.Name, out Expression? errant))
            {
                throw new EnvironmentBindingException(sym.Name, errant.ToString());
            }
            _bindings.Add(sym.Name, definition);
        }

        //public void ExtendMany(Expression keyList, Expression defList)
        //{
        //    if (!keyList.IsNil && !defList.IsNil)
        //    {
        //        Extend(keyList.ExpectCar().ExpectSymbol(), defList.ExpectCar());
        //        ExtendMany(keyList.ExpectCdr(), defList.ExpectCdr());
        //    }
        //    else if (keyList.ExpectCdr().IsNil ^ keyList.ExpectCdr().IsNil)
        //    {
        //        throw new Exception("Mismatched list arg lengths in environment extension");
        //    }
        //}

        public void Set(Symbol sym, Expression definition)
        {
            _bindings[sym.Name] = definition;
        }

        public Expression Lookup(Symbol sym)
        {
            if (_bindings.TryGetValue(sym.Name, out Expression? expr) && expr is not null)
            {
                return expr;
            }
            else if (_lexicalContext is not null)
            {
                return _lexicalContext.Lookup(sym);
            }
            else
            {
                throw new EnvironmentLookupException(sym.Name);
            }
        }

        #endregion

        private void Print(string s)
        {
            if (OutputStream is null)
            {
                throw new Exception("Tried to write to non-existent output stream");
            }
            OutputStream.Write($"~ {s}");
        }


        #region Standard Environment

        public static Environment StdEnv(StreamWriter? writer = null)
        {
            Environment std = new(writer);

            std.BindSpecialForm("if");
            std.BindSpecialForm("cond");
            std.BindSpecialForm("case");
            std.BindSpecialForm("quote");
            std.BindSpecialForm("define");
            std.BindSpecialForm("set!");
            std.BindSpecialForm("eq?");
            std.BindSpecialForm("car");
            std.BindSpecialForm("cdr");
            std.BindSpecialForm("cons");
            std.BindSpecialForm("lambda");
            std.BindSpecialForm("let");

            std.BindProcedure(StdOps.Add);
            std.BindProcedure(StdOps.Subtract);
            std.BindProcedure(StdOps.Multiply);
            std.BindProcedure(StdOps.Divide);
            std.BindProcedure(StdOps.Modulo);
            std.BindProcedure(StdOps.Expt);
            std.BindProcedure(StdOps.AbsoluteValue);

            std.BindProcedure(StdOps.NumEqual);
            std.BindProcedure(StdOps.NumGreater);
            std.BindProcedure(StdOps.NumLesser);
            std.BindProcedure(StdOps.NumGEq);
            std.BindProcedure(StdOps.NumLEq);
            std.BindProcedure(StdOps.NumNotEqual);

            std.BindProcedure(StdOps.NumMax);
            std.BindProcedure(StdOps.NumMin);

            std.BindProcedure(StdOps.And);
            std.BindProcedure(StdOps.Or);
            std.BindProcedure(StdOps.Xor);
            std.BindProcedure(StdOps.Not);

            std.BindProcedure(StdOps.IsAtom);
            std.BindProcedure(StdOps.IsSymbol);
            std.BindProcedure(StdOps.IsProcedure);
            std.BindProcedure(StdOps.IsNumber);
            std.BindProcedure(StdOps.IsList);
            std.BindProcedure(StdOps.IsNil);
            std.BindProcedure(StdOps.IsPair);

            std.BindProcedure(StdOps.Eqv);
            std.BindProcedure(StdOps.Equal);

            std.BindProcedure(StdOps.Eval);
            std.BindProcedure(StdOps.Apply);
            std.BindProcedure(StdOps.Begin);
            std.BindProcedure(StdOps.List);
            std.BindProcedure(StdOps.ListStar);
            std.BindProcedure(StdOps.Length);
            std.BindProcedure(StdOps.Append);
            std.BindProcedure(StdOps.Map);

            std.BindProcedure(StdOps.Caar );
            std.BindProcedure(StdOps.Cadr );
            std.BindProcedure(StdOps.Cdar );
            std.BindProcedure(StdOps.Cddr );
            std.BindProcedure(StdOps.Caaar);
            std.BindProcedure(StdOps.Caadr);
            std.BindProcedure(StdOps.Cadar);
            std.BindProcedure(StdOps.Caddr);
            std.BindProcedure(StdOps.Cdaar);
            std.BindProcedure(StdOps.Cdadr);
            std.BindProcedure(StdOps.Cddar);
            std.BindProcedure(StdOps.Cdddr);

            return std;
        }

        private void BindSpecialForm(string formName)
        {
            Symbol sym = new(formName);
            Extend(sym, sym);
        }

        private void BindProcedure(Procedure proc)
        {
            Extend(new(proc.Name), proc);
        }

        #endregion
    }
}
