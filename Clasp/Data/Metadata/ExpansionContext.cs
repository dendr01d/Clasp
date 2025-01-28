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
        public Environment CurrentEnv { get; private set; }
        public BlockScope CurrentBlock { get; private set; }

        public int Phase { get; private set; }

        /// <summary>IDs of scopes that were introduced for the purpose of binding identifiers</summary>
        private readonly HashSet<uint> _bindingScopes;

        /// <summary>IDs of scopes that were introduced during macro use/introduction.</summary>
        private readonly HashSet<uint> _macroScopes;

        /// <summary>True iff the current expansion should stop at core forms.</summary>
        public bool RestrictedToImmediate { get; private set; }

        private readonly ScopeTokenGenerator _gen;


        private ExpansionContext(
            Environment env,
            BlockScope scp,
            int phase,
            IEnumerable<uint> bindingScopes,
            IEnumerable<uint> macroScopes,
            bool restrictToImmediate,
            ScopeTokenGenerator gen)
        {
            CurrentEnv = env;
            CurrentBlock = scp;
            Phase = phase;

            _bindingScopes = new(bindingScopes);
            _macroScopes = new(macroScopes);

            RestrictedToImmediate = restrictToImmediate;

            _gen = gen;
        }

        public static ExpansionContext FreshExpansion(Environment env, ScopeTokenGenerator gen)
        {
            return new ExpansionContext(env, BlockScope.MakeTopLevel(gen),
                1,
                [],
                [],
                false,
                gen);
        }
        
        public ExpansionContext ExpandInNewPhase()
        {
            return new ExpansionContext(
                Binding.StandardEnv.CreateNew(),
                BlockScope.MakeTopLevel(_gen),
                Phase + 1,
                [],
                _macroScopes,
                false,
                _gen);
        }

        public ExpansionContext ExpandInNestedBlock()
        {
            return new ExpansionContext(
                CurrentEnv.Enclose(),
                BlockScope.MakeBody(_gen, CurrentBlock),
                Phase,
                [],
                _macroScopes,
                RestrictedToImmediate,
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
            ExpansionBinding[] candidates = CurrentBlock.ResolveBindings(id.SymbolicName, id.GetScopeSet(Phase)).ToArray();

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
            ExpansionBinding[] candidates = CurrentBlock.ResolveBindings(id.SymbolicName, id.GetScopeSet(Phase)).ToArray();

            if (candidates.Length == 1)
            {
                binding = candidates.Single();
                return true;
            }

            binding = null;
            return false;
        }

        #region Env Helpers

        public Term Dereference(Identifier id) => CurrentEnv.LookUp(id.SymbolicName);
        public Term Dereference(ExpansionBinding binding) => Dereference(binding.BoundId);

        public void RenameVariable(Identifier symId, IEnumerable<uint> scopeSet, Identifier bindingId)
        {
            ExpansionBinding newBinding = new ExpansionBinding(bindingId, BindingType.Variable);
            CurrentBlock.AddBinding(symId.SymbolicName, scopeSet, newBinding);
        }

        public void BindMacro(string bindingName, MacroProcedure macro)
        {
            CurrentEnv[bindingName] = macro;
        }

        public bool TryGetMacro(string bindingName,
            [NotNullWhen(true)] out MacroProcedure? macro)
        {
            if (CurrentEnv.TryGetValue(bindingName, out Term? value)
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
