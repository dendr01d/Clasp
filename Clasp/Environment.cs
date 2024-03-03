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

        public void Set(Symbol sym, Expression definition)
        {
            _bindings[sym.Name] = definition;
            //if (_lexicalContext is not null)
            //{
            //    _lexicalContext.Set(sym, definition);
            //}
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

            foreach(SpecialForm form in SpecialForm.Forms)
            {
                std.BindOp(form);
            }

            std.BindOp(StdOps.Add);
            std.BindOp(StdOps.Subtract);
            std.BindOp(StdOps.Multiply);
            std.BindOp(StdOps.Divide);
            std.BindOp(StdOps.Modulo);
            std.BindOp(StdOps.Expt);
            std.BindOp(StdOps.AbsoluteValue);

            std.BindOp(StdOps.NumEqual);
            std.BindOp(StdOps.NumGreater);
            std.BindOp(StdOps.NumLesser);
            std.BindOp(StdOps.NumGEq);
            std.BindOp(StdOps.NumLEq);
            std.BindOp(StdOps.NumNotEqual);

            std.BindOp(StdOps.NumMax);
            std.BindOp(StdOps.NumMin);

            std.BindOp(StdOps.Xor);
            std.BindOp(StdOps.Not);

            std.BindOp(StdOps.IsAtom);
            std.BindOp(StdOps.IsSymbol);
            std.BindOp(StdOps.IsProcedure);
            std.BindOp(StdOps.IsNumber);
            std.BindOp(StdOps.IsList);
            std.BindOp(StdOps.IsNil);
            std.BindOp(StdOps.IsPair);

            //std.BindOp(StdOps.Eqv);
            std.BindOp(StdOps.Equal);

            std.BindOp(StdOps.Eval);
            std.BindOp(StdOps.Apply);
            std.BindOp(StdOps.List);
            std.BindOp(StdOps.ListStar);
            std.BindOp(StdOps.Length);
            std.BindOp(StdOps.Append);
            std.BindOp(StdOps.Map);

            std.BindOp(StdOps.Caar );
            std.BindOp(StdOps.Cadr );
            std.BindOp(StdOps.Cdar );
            std.BindOp(StdOps.Cddr );
            std.BindOp(StdOps.Caaar);
            std.BindOp(StdOps.Caadr);
            std.BindOp(StdOps.Cadar);
            std.BindOp(StdOps.Caddr);
            std.BindOp(StdOps.Cdaar);
            std.BindOp(StdOps.Cdadr);
            std.BindOp(StdOps.Cddar);
            std.BindOp(StdOps.Cdddr);

            return new Environment(std);
        }

        private void BindOp(Operator op)
        {
            Extend(new(op.Name), op);
        }

        #endregion
    }
}
