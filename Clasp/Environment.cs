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

        public Environment(StreamWriter streamOut) : this()
        {
            OutputStream = streamOut;
        }

        public Environment(Environment context) : this()
        {
            _lexicalContext = context;
            OutputStream = context.OutputStream;
        }

        public static Environment StdEnv(StreamWriter writer)
        {
            return new Environment(writer);
        }

        public void Extend(Symbol sym, Expression definition)
        {
            if (_bindings.TryGetValue(sym.Name, out Expression? errant))
            {
                throw new Exception($"Tried to re-bind symbol {sym} bound to {errant}");
            }
            _bindings.Add(sym.Name, definition);
        }

        public void ExtendMany(Expression keyList, Expression defList)
        {
            if (!keyList.IsNil && !defList.IsNil)
            {
                Extend(keyList.ExpectCar().ExpectSymbol(), defList.ExpectCar());
                ExtendMany(keyList.ExpectCdr(), defList.ExpectCdr());
            }
            else if (keyList.ExpectCdr().IsNil ^ keyList.ExpectCdr().IsNil)
            {
                throw new Exception("Mismatched list arg lengths in environment extension");
            }
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
                throw new Exception($"Tried to reference un-bound symbol {sym}");
            }
        }

        private void Print(string s)
        {
            if (OutputStream is null)
            {
                throw new Exception("Tried to write to non-existent output stream");
            }
            OutputStream.Write($"~ {s}");
        }
    }
}
