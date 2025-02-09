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


        private uint? _insideEdge;
        private uint? _macroUseSite;


        /// <summary>Inform how certain terms should be expanded.</summary>
        public readonly SyntaxMode Mode;

        private readonly ScopeTokenGenerator _gen;

        private ExpansionContext(Environment env, BindingStore store, int phase,
            SyntaxMode mode, ScopeTokenGenerator gen,
            uint? insideEdge, uint? macroUseSite)
        {
            CompileTimeEnv = env;
            GlobalBindingStore = store;
            Phase = phase;

            Mode = mode;
            _gen = gen;

            _insideEdge = insideEdge;
            _macroUseSite = macroUseSite;
        }

        public static ExpansionContext FreshExpansion(Environment env, BindingStore bs, ScopeTokenGenerator gen)
        {
            return new ExpansionContext(
                env: env,
                store: bs,
                phase: 1,
                mode: SyntaxMode.TopLevel,
                gen: gen,
                insideEdge: null,
                macroUseSite: null);
        }
        
        public ExpansionContext InNewPhase()
        {
            return new ExpansionContext(
                env: CompileTimeEnv.TopLevel.Enclose(),
                store: new BindingStore(),
                phase: Phase + 1,
                mode: SyntaxMode.TopLevel,
                gen: _gen,
                insideEdge: null,
                macroUseSite: null);
        }

        public ExpansionContext InSyntaxMode(SyntaxMode context)
        {
            return new ExpansionContext(
                env: CompileTimeEnv,
                store: GlobalBindingStore,
                phase: Phase ,
                mode: context,
                gen: _gen,
                insideEdge: _insideEdge,
                macroUseSite: _macroUseSite);
        }

        public ExpansionContext InBody(uint insideEdge)
        {
            return new ExpansionContext(
                env: CompileTimeEnv.Enclose(),
                store: GlobalBindingStore,
                phase: Phase,
                mode: Mode,
                gen: _gen,
                insideEdge: insideEdge,
                macroUseSite: null);
        }

        public ExpansionContext AsMacroResult(uint useSiteScope)
        {
            return new ExpansionContext(
                env: CompileTimeEnv.Enclose(),
                store: GlobalBindingStore,
                phase: Phase,
                mode: Mode,
                gen: _gen,
                insideEdge: _insideEdge,
                macroUseSite: useSiteScope);
        }

        #region Scope Manipulation

        public uint FreshScopeToken()
        {
            return _gen.FreshToken();
        }

        public void AddScope(Syntax stx, params uint[] scopeTokens) => stx.AddScope(Phase, scopeTokens);
        public void FlipScope(Syntax stx, params uint[] scopeTokens) => stx.FlipScope(Phase, scopeTokens);
        public void RemoveScope(Syntax stx, params uint[] scopeTokens) => stx.RemoveScope(Phase, scopeTokens);

        public void AddPendingInsideEdgeScope(Syntax stx)
        {
            if (_insideEdge is uint insideEdge)
            {
                stx.AddScope(Phase, insideEdge);
            }
        }

        public void SanitizeIdentifier(Identifier key)
        {
            if (_insideEdge is uint insideEdge)
            {
                key.AddScope(Phase, insideEdge);
            }

            if (_macroUseSite is uint macroUseSite)
            {
                key.RemoveScope(Phase, macroUseSite);
            }
        }

        #endregion


        #region Binding and Lookup

        /// <summary>
        /// Create a unique <see cref="Identifier"/> corresponding to the scope of
        /// <paramref name="symbolicId"/> and the current <see cref="Phase"/>, then record
        /// it as a <see cref="BindingType.Variable"/>.
        /// </summary>
        /// <returns>The renamed <see cref="Identifier"/>.</returns>
        public Identifier BindVariable(Identifier symbolicId)
        {
            Identifier bindingId = new Identifier(new GenSym(symbolicId.Name), symbolicId.LexContext);

            CompileTimeBinding binding = new CompileTimeBinding(bindingId, BindingType.Variable);
            GlobalBindingStore.AddBinding(symbolicId, Phase, binding);

            return bindingId;
        }

        /// <summary>
        /// Create a unique <see cref="Identifier"/> corresponding to the scope of
        /// <paramref name="symbolicId"/> and the current <see cref="Phase"/>, then record
        /// it as a <see cref="BindingType.Transformer"/>, while binding it to <paramref name="macro"/>
        /// within the <see cref="CompileTimeEnv"/>.
        /// </summary>
        /// <returns>The renamed <see cref="Identifier"/>.</returns>
        public Identifier BindMacro(Identifier symbolicId, MacroProcedure macro)
        {
            Identifier bindingId = new Identifier(new GenSym(symbolicId.Name), symbolicId.LexContext);

            CompileTimeBinding binding = new CompileTimeBinding(bindingId, BindingType.Transformer);
            GlobalBindingStore.AddBinding(symbolicId, Phase, binding);
            CompileTimeEnv[bindingId.Name] = macro;

            return bindingId;
        }

        /// <summary>
        /// Explicitly record <paramref name="bindingId"/> as a <see cref="BindingType.Special"/>
        /// corresponding to the scope of <paramref name="symbolicId"/> and the current <see cref="Phase"/>.
        /// </summary>
        public void BindSpecial(Identifier symbolicId, Identifier bindingId)
        {
            CompileTimeBinding binding = new CompileTimeBinding(bindingId, BindingType.Special);
            GlobalBindingStore.AddBinding(symbolicId, Phase, binding);
            //CompileTimeEnv[bindingId.Name] = bindingId.Expose();
        }

        /// <summary>
        /// Attempt to look up the binding information corresponding to <paramref name="id"/>
        /// within its given scope and the current <see cref="Phase"/>.
        /// </summary>
        public bool TryResolveBinding(Identifier id, [NotNullWhen(true)] out CompileTimeBinding? binding)
        {
            CompileTimeBinding[] candidates = GlobalBindingStore.ResolveBindings(id.Name, id.LexContext[Phase]).ToArray();

            if (candidates.Length == 1)
            {
                binding = candidates.Single();
                return true;
            }

            binding = null;
            return false;
        }

        public bool TryDereferenceBinding(CompileTimeBinding binding, [NotNullWhen(true)] out Term? boundValue)
        {
            return CompileTimeEnv.TryGetValue(binding.Name, out boundValue);
        }

        public bool TryDereferenceMacro(CompileTimeBinding binding, [NotNullWhen(true)] out MacroProcedure? boundMacro)
        {
            if (binding.BoundType == BindingType.Transformer
                && TryDereferenceBinding(binding, out Term? maybeMacro)
                && maybeMacro is MacroProcedure mp)
            {
                boundMacro = mp;
                return true;
            }

            boundMacro = null;
            return false;
        }

        //public bool TryDereferenceSpecial(CompileTimeBinding binding, [NotNullWhen(true)] out Symbol? boundSymbol)
        //{
        //    if (binding.BoundType == BindingType.Special
        //        && TryDereferenceBinding(binding, out Term? maybeSpecial)
        //        && maybeSpecial is Symbol sym)
        //    {
        //        boundSymbol = sym;
        //        return true;
        //    }

        //    boundSymbol = null;
        //    return false;
        //}

        #endregion
    }
}
