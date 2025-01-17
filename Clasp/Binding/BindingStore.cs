using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;

using ScopeSet = System.Collections.Generic.HashSet<uint>;
using ScopeMap = System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<System.Collections.Generic.HashSet<uint>, string>>;

namespace Clasp.Binding
{
    internal class BindingStore
    {
        private Dictionary<string, ScopeMap> _renamesByScope;
        private int _count;

        public BindingStore()
        {
            _renamesByScope = new Dictionary<string, List<KeyValuePair<HashSet<uint>, string>>>();
            _count = 0;
        }

        /// <summary>
        /// Given a collection of sets-of-scopes mapped to binding names, find the subset of binding names
        /// whose keys form the largest subset of <paramref name="keys"/>
        /// </summary>
        private static string[] IndexBySubset(ScopeMap map, IEnumerable<uint> keys)
        {
            // group all entries in the map according to subset size
            // then pick the group with the biggest key

            return map.GroupBy(x => x.Key.Intersect(keys).Count())
                .MaxBy(x => x.Key)
                ?.Select(x => x.Value)
                .ToArray()
                ?? Array.Empty<string>();
        }

        /// <summary>
        /// Attempt to resolve the binding name of the given identifier.
        /// </summary>
        /// <param name="stx">The identifier to be resolved. The <see cref="Syntax"/> must be wrapped around a <see cref="Symbol"/>.</param>
        /// <param name="phase">The expansion phase in which the resolution is being performed.</param>
        /// <returns>
        /// The binding name of the identifier, assuming <paramref name="stx"/> is really an identifier,
        /// <paramref name="stx"/> is truly bound in this <see cref="BindingStore"/>,
        /// and the scope set of <paramref name="stx"/> doesn't ambiguously point to multiple bindings.
        /// </returns>
        /// <exception cref="ExpanderException.UnboundIdentifier"></exception>
        /// <exception cref="ExpanderException.AmbiguousIdentifier"></exception>
        /// <exception cref="ClaspGeneralException"></exception>
        public string ResolveBindingName(Syntax stx, int phase)
        {
            if (stx is Syntax<Symbol> stxSym)
            {
                string symbolicName = stxSym.Expose.Name;

                if (_renamesByScope.TryGetValue(stxSym.Expose.Name, out ScopeMap? map))
                {
                    string[] candidates = IndexBySubset(map, stx.GetContext(phase));

                    if (candidates.Length == 0)
                    {
                        throw new ExpanderException.UnboundIdentifier(symbolicName, stx);
                    }
                    else if (candidates.Length > 1)
                    {
                        throw new ExpanderException.AmbiguousIdentifier(symbolicName, stx);
                    }
                    else
                    {
                        return candidates[0];
                    }
                }
            }

            throw new ClaspGeneralException("Tried to resolve rename binding of non-identifier: {0}", stx);
        }

        /// <summary>
        /// Attempt to bind the symbolic name of the identifier <paramref name="stx"/> to
        /// the given <paramref name="bindingName"/> within the set-of-scopes of <paramref name="stx"/>
        /// corresponding to the given expansion <paramref name="phase"/>.
        /// </summary>
        /// <param name="stx">The identifier to be resolved. The <see cref="Syntax"/> must be wrapped around a <see cref="Symbol"/>.</param>
        /// <param name="phase">The expansion phase in which the rename binding is being performed.</param>
        /// <param name="bindingName">
        /// The name that the identifier <paramref name="stx"/> should assume in the current set-of-scopes.
        /// </param>
        /// <exception cref="ClaspGeneralException"></exception>
        public void RenameInScope(Syntax stx, int phase, string bindingName)
        {
            if (stx is Syntax<Symbol> stxSym)
            {
                string symbolicName = stxSym.Expose.Name;

                if (stx.GetContext(phase) is HashSet<uint> symScope && symScope.Count > 1)
                {
                    if (!_renamesByScope.ContainsKey(symbolicName))
                    {
                        _renamesByScope[symbolicName] = new ScopeMap();
                    }

                    var binding = new KeyValuePair<HashSet<uint>, string>(symScope, bindingName);
                    _renamesByScope[symbolicName].Add(binding);

                    ++_count;
                }
            }
            else
            {
                throw new ClaspGeneralException("Tried to create rename binding of non-identifier: {0}", stx);
            }
        }


        #region IDictionary Implementation

        public string this[Syntax key, int phase]
        {
            get => ResolveBindingName(key, phase);
            set => RenameInScope(key, phase, value);
        }

        public int Count => _count;

        public void Add(Syntax key, int phase, string value) => RenameInScope(key, phase, value);

        //public bool Remove(Syntax key)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}
