using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Environments;
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
        public readonly Environment CompileTimeEnv;

        /// <summary>
        /// Globally records renamings of identifiers for the current expansion.
        /// </summary>
        public readonly BindingStore GlobalBindingStore;

        public readonly int Phase;

        /// <summary>IDs of scopes that were introduced during macro use/introduction.</summary>
        private readonly HashSet<uint> _macroScopes;

        /// <summary>Inform how certain terms should be expanded.</summary>
        public readonly SyntaxMode Mode;

        private readonly ScopeTokenGenerator _gen;

        private ExpansionContext(
            Environment env,
            BindingStore scp,
            int phase,
            IEnumerable<uint> macroScopes,
            SyntaxMode mode,
            ScopeTokenGenerator gen)
        {
            CompileTimeEnv = env;
            GlobalBindingStore = scp;
            Phase = phase;

            _macroScopes = new(macroScopes);

            Mode = mode;

            _gen = gen;
        }

        public static ExpansionContext FreshExpansion(Environment env, ScopeTokenGenerator gen)
        {
            return new ExpansionContext(
                StandardEnv.CreateNew(),
                new BindingStore(),
                1,
                [],
                SyntaxMode.TopLevel,
                gen);
        }
        
        public ExpansionContext ExpandInNewPhase()
        {
            return new ExpansionContext(
                CompileTimeEnv.TopLevel.Enclose(),
                new BindingStore(),
                Phase + 1,
                [],
                SyntaxMode.TopLevel,
                _gen);
        }

        public ExpansionContext ExpandInMode(SyntaxMode context)
        {
            return new ExpansionContext(
                CompileTimeEnv,
                GlobalBindingStore,
                Phase,
                _macroScopes,
                context,
                _gen);
        }

        public ExpansionContext ExpandInSubBlock()
        {
            return new ExpansionContext(
                CompileTimeEnv.Enclose(),
                GlobalBindingStore,
                Phase,
                _macroScopes,
                Mode,
                _gen);
        }

        public uint TokenizeScope()
        {
            return _gen.FreshToken();
        }

        public uint TokenizeMacroScope()
        {
            uint output = _gen.FreshToken();
            _macroScopes.Add(output);
            return output;
        }

        public CompileTimeBinding ResolveBinding(Identifier id)
        {
            CompileTimeBinding[] candidates = GlobalBindingStore.ResolveBindings(id.Name, id.GetScopeSet(Phase)).ToArray();

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

        public bool TryResolveBinding(Identifier id, [NotNullWhen(true)] out CompileTimeBinding? binding)
        {
            CompileTimeBinding[] candidates = GlobalBindingStore.ResolveBindings(id.Name, id.GetScopeSet(Phase)).ToArray();

            if (candidates.Length == 1)
            {
                binding = candidates.Single();
                return true;
            }

            binding = null;
            return false;
        }

        #region Scope Manipulation

        public void AddScope(Syntax stx, params uint[] scopeTokens) => stx.AddScope(Phase, scopeTokens);
        public void FlipScope(Syntax stx, params uint[] scopeTokens) => stx.FlipScope(Phase, scopeTokens);
        public void RemoveScope(Syntax stx, params uint[] scopeTokens) => stx.RemoveScope(Phase, scopeTokens);

        #endregion


        #region Env Helpers

        public Term Dereference(Identifier id) => CompileTimeEnv.LookUp(id.Name);
        public Term Dereference(CompileTimeBinding binding) => Dereference(binding.BoundId);

        public void BindVariable(Identifier symId, Identifier bindingId)
        {
            CompileTimeBinding binding = new CompileTimeBinding(bindingId, BindingType.Variable);
            GlobalBindingStore.AddBinding(symId, Phase, binding);
        }

        public void BindMacro(Identifier symId, Identifier bindingId, MacroProcedure macro)
        {
            CompileTimeBinding binding = new CompileTimeBinding(bindingId, BindingType.Transformer);
            GlobalBindingStore.AddBinding(symId, Phase, binding);
            CompileTimeEnv[bindingId.Name] = macro;
        }

        public void BindSpecial(Identifier symId, Identifier bindingId, Symbol keyword)
        {
            CompileTimeBinding binding = new CompileTimeBinding(bindingId, BindingType.Special);
            GlobalBindingStore.AddBinding(symId, Phase, binding);
            CompileTimeEnv[bindingId.Name] = keyword;
        }

        public bool TryGetMacro(string bindingName,
            [NotNullWhen(true)] out MacroProcedure? macro)
        {
            if (CompileTimeEnv.TryGetValue(bindingName, out Term? value)
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
