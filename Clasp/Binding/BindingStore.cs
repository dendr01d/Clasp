using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;

namespace Clasp.Binding
{
    internal class BindingStore
    {
        private Dictionary<string, ScopeMap> _bindingLookup;

        public BindingStore()
        {
            _bindingLookup = new Dictionary<string, ScopeMap>();
        }

        public void BindName(string symbolicName, ScopeSet newSet, string bindingName)
        {
            if (_bindingLookup.TryGetValue(symbolicName, out ScopeMap? map))
            {
                map.AddMapping(newSet, bindingName);
            }
            else
            {
                _bindingLookup[symbolicName] = new ScopeMap();
                _bindingLookup[symbolicName].AddMapping(newSet, bindingName);
            }
        }

        public bool NameIsBound(string symbolicName, ScopeSet setOfScopes)
        {
            if (_bindingLookup.TryGetValue(symbolicName, out ScopeMap? map))
            {
                return map.LookupLargestSubset(setOfScopes).Length > 0;
            }

            return false;
        }

        public string ResolveBindingName(string symbolicName, ScopeSet setOfScopes, Syntax identifier)
        {
            if (_bindingLookup.TryGetValue(symbolicName, out ScopeMap? map))
            {
                KeyValuePair<ScopeSet, string>[] matches = map.LookupLargestSubset(setOfScopes);

                if (matches.Length == 0)
                {
                    // i.e. no scope sets matched
                    throw new ExpanderException.UnboundIdentifier(symbolicName, identifier);
                }
                else if (matches.Length > 1)
                {
                    // multiple scope sets ambiguously matched
                    throw new ExpanderException.AmbiguousIdentifier(symbolicName, identifier);
                }
                else
                {
                    return matches[0].Value;
                }
            }

            return symbolicName;
        }

    }
}
