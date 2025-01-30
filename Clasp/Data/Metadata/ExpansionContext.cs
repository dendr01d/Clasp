using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Binding.Scopes;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Syntax;

namespace Clasp.Data.Metadata
{
    /// <summary>
    /// Manages state information of an ongoing program expansion
    /// </summary>
    internal sealed class ExpansionContext
    {
        /// <summary>
        /// Records compile-time bindings of identifiers in the current lexical scope. Closures
        /// are used to ensure the locality of bound values.
        /// </summary>
        public readonly Environment ExpTimeEnv;

        /// <summary>
        /// Globally records renamings of identifiers for the current expansion.
        /// </summary>
        public readonly BindingStore GlobalBindingStore;

        public readonly int Phase;

        /// <summary>IDs of scopes that were introduced during macro use/introduction.</summary>
        private readonly HashSet<uint> _macroScopes;

        /// <summary>Encodes what kind of terms are permitted in the current context.</summary>
        public readonly ExpansionMode Mode;

        private readonly ScopeTokenGenerator _gen;

        private ExpansionContext(
            Environment env,
            BindingStore scp,
            int phase,
            IEnumerable<uint> macroScopes,
            ExpansionMode mode,
            ScopeTokenGenerator gen)
        {
            ExpTimeEnv = env;
            GlobalBindingStore = scp;
            Phase = phase;

            _macroScopes = new(macroScopes);

            Mode = mode;

            _gen = gen;
        }

        public static ExpansionContext FreshExpansion(Environment env, ScopeTokenGenerator gen)
        {
            return new ExpansionContext(
                env,
                new BindingStore(),
                1,
                [],
                ExpansionMode.TopLevel,
                gen);
        }
        
        public ExpansionContext ExpandInNewPhase()
        {
            return new ExpansionContext(
                ExpTimeEnv.TopLevel,
                new BindingStore(),
                Phase + 1,
                [],
                ExpansionMode.TopLevel,
                _gen);
        }

        public ExpansionContext ExpandInNewMode(ExpansionMode context)
        {
            return new ExpansionContext(
                ExpTimeEnv.Enclose(),
                GlobalBindingStore,
                Phase,
                _macroScopes,
                context,
                _gen);
        }

        public uint TokenizeMacroScope()
        {
            uint output = _gen.FreshToken();
            _macroScopes.Add(output);
            return output;
        }

        public ExpansionBinding ResolveBinding(Identifier id)
        {
            ExpansionBinding[] candidates = GlobalBindingStore.ResolveBindings(id.SymbolicName, id.GetScopeSet(Phase)).ToArray();

            if (candidates.Length == 0)
            {
                throw new ExpanderException.UnboundIdentifier(id);
            }
            else if (candidates.Length > 1)
            {
                throw new ExpanderException.AmbiguousIdentifier(id, candidates);
            }
            else
            {
                return candidates.Single();
            }
        }

        public bool TryResolveBinding(Identifier id, [NotNullWhen(true)] out ExpansionBinding? binding)
        {
            ExpansionBinding[] candidates = GlobalBindingStore.ResolveBindings(id.SymbolicName, id.GetScopeSet(Phase)).ToArray();

            if (candidates.Length == 1)
            {
                binding = candidates.Single();
                return true;
            }

            binding = null;
            return false;
        }

        #region Scope Manipulation

        public void Paint(Syntax stx, params uint[] scopes) => ScopeAdjuster.Paint(stx, Phase, scopes);
        public void Flip(Syntax stx, params uint[] scopes) => ScopeAdjuster.Flip(stx, Phase, scopes);
        public void Remove(Syntax stx, params uint[] scopes) => ScopeAdjuster.Flip(stx, Phase, scopes);

        #endregion


        #region Env Helpers

        public Term Dereference(Identifier id) => ExpTimeEnv.LookUp(id.SymbolicName);
        public Term Dereference(ExpansionBinding binding) => Dereference(binding.BoundId);

        public void BindVariable(Identifier symId, Identifier bindingId)
        {
            ExpansionBinding binding = new ExpansionBinding(bindingId, BindingType.Variable);
            GlobalBindingStore.AddBinding(symId, Phase, binding);
        }

        public void BindMacro(Identifier symId, Identifier bindingId, MacroProcedure macro)
        {
            ExpansionBinding binding = new ExpansionBinding(bindingId, BindingType.Transformer);
            GlobalBindingStore.AddBinding(symId, Phase, binding);
            ExpTimeEnv[bindingId.SymbolicName] = macro;
        }

        public void BindSpecial(Identifier symId, Identifier bindingId, Symbol keyword)
        {
            ExpansionBinding binding = new ExpansionBinding(bindingId, BindingType.Special);
            GlobalBindingStore.AddBinding(symId, Phase, binding);
            ExpTimeEnv[bindingId.SymbolicName] = keyword;
        }

        public bool TryGetMacro(string bindingName,
            [NotNullWhen(true)] out MacroProcedure? macro)
        {
            if (ExpTimeEnv.TryGetValue(bindingName, out Term? value)
                && value is MacroProcedure result)
            {
                macro = result;
                return true;
            }

            macro = null;
            return false;
        }

        #endregion
    }
}
