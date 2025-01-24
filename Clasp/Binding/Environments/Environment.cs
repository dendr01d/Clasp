using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Data.Terms;

namespace Clasp.Binding.Environments
{
    internal abstract class Environment
    {
        protected readonly Dictionary<string, Term> _mutableBindings;
        public readonly int Depth;

        public abstract SuperEnvironment TopLevel { get; }
        public abstract bool IsTopLevel { get; }

        protected Environment(int depth)
        {
            _mutableBindings = new Dictionary<string, Term>();
            Depth = depth;
        }

        #region Utilities

        public SubEnvironment Enclose() => new SubEnvironment(this);

        public abstract Term LookUp(string name);

        protected abstract IEnumerable<Environment> EnumerateScope();

        private IEnumerable<KeyValuePair<string, Term>> EnumerateAccessibleBindings()
        {
            return EnumerateScope()
                .SelectMany(x => x._mutableBindings)
                .DistinctBy(x => x.Key);
        }

        //public Environment ExtractCompileTimeEnv()
        //{
        //    Environment output = new Environment(EnumerateAccessibleBindings()
        //        .Where(x => x.Value is Variable || x.Value is Fixed));

        //    output.Add(Symbol.Lambda.Name, Symbol.Lambda);
        //    output.Add(Symbol.Define.Name, Symbol.Define);
        //    output.Add(Symbol.DefineSyntax.Name, Symbol.DefineSyntax);
        //    output.Add(Symbol.Quote.Name, Symbol.Quote);
        //    output.Add(Symbol.Syntax.Name, Symbol.Syntax);

        //    return output;
        //}

        #endregion

        #region IDictionary Implementation

        public Term this[string key]
        {
            get => LookUp(key);
            set => _mutableBindings[key] = value;
        }

        public abstract bool ContainsKey(string key);

        public abstract bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value);

        #endregion
    }
}
