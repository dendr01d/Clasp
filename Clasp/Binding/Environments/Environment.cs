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

        public abstract SuperEnvironment GlobalEnv { get; }

        protected Environment(int depth)
        {
            _mutableBindings = new Dictionary<string, Term>();
            Depth = depth;
        }

        #region Utilities

        public SubEnvironment Enclose() => new SubEnvironment(this);
        public abstract Term LookUp(string name);

        public bool Binds(string name) => ContainsKey(name);


        #endregion

        #region IDictionary Implementation

        public Term this[string key]
        {
            get => LookUp(key);
            set => _mutableBindings[key] = value;
        }

        public abstract bool ContainsKey(string key);

        public abstract bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value);

        public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T? value)
            where T : Term
        {
            if (TryGetValue(key, out Term? bound) && bound is T output)
            {
                value = output;
                return true;
            }

            value = null;
            return false;
        }

        #endregion
    }
}
