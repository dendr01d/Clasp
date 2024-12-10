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

        public string ResolveBindingName(string symbolicName, ScopeSet setOfScopes)
        {
            if (_bindingLookup.TryGetValue(symbolicName, out ScopeMap? map))
            {
                KeyValuePair<ScopeSet, string>[] matches = map.LookupLargestSubset(setOfScopes);

                if (matches.Length == 0)
                {
                    // i.e. no scope sets matched
                    throw new ExpanderException.BindingResolution(symbolicName, setOfScopes);
                }
                else if (matches.Length > 1)
                {
                    // multiple scope sets ambiguously matched
                    throw new ExpanderException.BindingResolution(symbolicName, setOfScopes, matches);
                }
                else
                {
                    return matches[0].Value;
                }
            }

            return symbolicName;
        }

        //public string ResolveName(Identifier id, int phaseLevel)
        //{
        //    if (id.Context.TryGetValue(phaseLevel, out ScopeSet? ss))
        //    {
        //        return ResolveName(id.Name, ss);
        //    }
        //    throw new ClaspException.Uncategorized("Identifier '{0}' is unbound at given phase level {1}.", id, phaseLevel);
        //}

        //public string ResolveName(string name, ScopeSet context)
        //{
        //}

        //public AstNode? ResolveBinding(Identifier id, int phaseLevel, Binding.Environment env)
        //{
        //    if (id.Context.TryGetValue(phaseLevel, out ScopeSet? ss))
        //    {
        //        string resolvedName = ResolveName(id.WrappedValue.Name, ss);

        //        if (env.TryGetValue(resolvedName, out AstNode? output))
        //        {
        //            return output;
        //        }
        //    }

        //    return null;
        //}

    }
}
