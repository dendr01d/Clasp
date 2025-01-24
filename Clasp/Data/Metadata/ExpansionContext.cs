using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding.Environments;
using Clasp.Binding.Scopes;
using Clasp.Data.Terms;

namespace Clasp.Data.Metadata
{
    internal sealed class ExpansionContext
    {
        #region Object Members

        public Environment CurrentEnv { get; private set; }


        //public Scope CurrentScope { get; private set; }
        // this can't really be tracked this way (like an extending tree structure)
        // because scopes can be freely created and added/flipped/removed in the case of e.g. macro invocation
        // so there's not a 1-1 correspondence between lexical scopes and expansion contexts(!)

        public int Phase { get; private set; }

        /// <summary>IDs of scopes that were introduced for the purpose of binding identifiers</summary>
        private readonly HashSet<uint> _bindingScopes;

        /// <summary>IDs of scopes that were introduced during macro use/introduction.</summary>
        private readonly HashSet<uint> _macroScopes;

        public ExpandingType CurrentContext { get; private set; }

        /// <summary>True iff the current expansion should stop at core forms.</summary>
        public bool RestrictedToImmediate { get; private set; }

        private readonly ScopeFactory _factory;

        #endregion

        #region Constructors

        // Full Constructor
        private ExpansionContext(
            Environment env,
            Scope scp,
            int phase,
            ScopeFactory factory,
            IEnumerable<uint> bindingScopes,
            IEnumerable<uint> macroScopes,
            ExpandingType currentCtx,
            bool restrictToImmediate)
        {
            CurrentEnv = env;
            CurrentScope = scp;
            Phase = phase;

            _bindingScopes = new(bindingScopes);
            _macroScopes = new(macroScopes);

            CurrentContext = currentCtx;
            RestrictedToImmediate = restrictToImmediate;

            _factory = factory;
        }

        // Private Copy Constructor

        // Public Restricted Constructor

        #endregion

        #region Context Extension

        //public ExpansionContext GetFresh() => new ExpansionContext(Binding.StandardEnv.CreateNew(), _factory.NewScope(),
        //    1, _factory,
        //    )

        public ExpansionContext GetNextPhaseContext()
        {
            return new ExpansionContext(
                Binding.StandardEnv.CreateNew(),
                _factory.NewScope(),
                Phase + 1,
                _factory,
                CurrentScope.ScopeSet,
                [],
                ExpandingType.TopLevel,
                false);
        }

        //public ExpansionContext GetSubBindingContext(ExpandingType expandingNext)
        //{
        //    Scope sub = _factory.NewScopeInside(CurrentScope);

        //    return new ExpansionContext(
        //        Binding.StandardEnv.CreateNew(),
        //        sub,
        //        Phase,
        //        _factory,
        //        _bindingScopes.Append(sub.Id),
        //        [],
        //        expandingNext,
        //        false);
        //}

        public void RecordMacroUse(Scope scp) 

        #endregion

        #region Context Mutation

        public void PaintScopeInCurrentPhase(Syntax stx, params uint[] scopes) => ScopeAdjuster.Paint(stx, Phase, scopes);
        public void FlipScopeInCurrentPhase(Syntax stx, params uint[] scopes) => ScopeAdjuster.Flip(stx, Phase, scopes);
        public void RemoveScopeInCurrentPhase(Syntax stx, params uint[] scopes) => ScopeAdjuster.Remove(stx, Phase, scopes);

        public void PaintScopeInAllPhases(Syntax stx, params uint[] scopes) => ScopeAdjuster.PaintAll(stx, scopes);
        public void FlipScopeInAllPhases(Syntax stx, params uint[] scopes) => ScopeAdjuster.FlipInAll(stx, scopes);
        public void RemoveScopeInAllPhases(Syntax stx, params uint[] scopes) => ScopeAdjuster.RemoveFromAll(stx, scopes);

        public Syntax StripBindingIdentifier(Syntax stx)
        {
            // eugh, don't like this
            Syntax output = Syntax.Wrap(stx.Expose(), stx);
            PaintScopeInCurrentPhase(output, _bindingScopes.ToArray());
            return output;
        }

        public Syntax StripQuotedSyntax(Syntax stx)
        {
            // or this
            Syntax output = Syntax.Wrap(stx.Expose(), stx); //this ought to be a deep copy instead
            RemoveScopeInCurrentPhase(output, _bindingScopes.Union(_macroScopes).ToArray());
            return output;
        }

        #endregion

        public string ResolveBindingName(Syntax stx, string symbolicName)
        {
            IEnumerable<string> matchedNames = CurrentScope.ResolveBindingNames(symbolicName, stx.GetScopeSet(Phase));
        }


        public string ResolveBindingName(Syntax stx, string symbolicName) => CurrentScope.ResolveBindingName(stx, symbolicName, Phase);
        public void RenameInCurrentScope(Syntax stx, string symbolicName, string bindingName) => CurrentScope.RenameInScope(stx, symbolicName, Phase, bindingName);

        public bool TryResolveBindingName(Syntax stx, string symbolicName,
            [NotNullWhen(true)] out string? bindingName)
        {
            return CurrentScope.TryResolveBindingName(stx, symbolicName, Phase, out bindingName);
        }

        //public string? MaybeDereferenceBinding(string bindingName)
        //{
        //    if (Env.TryGetValue(bindingName, out Term? deref)
        //        && deref is Syntax<Symbol> derefId)
        //    {
        //        return derefId.Expose.Name;
        //    }
        //    return null;
        //}

        #region Env Helpers

        private static readonly Symbol _variableMarker = new GenSym("variable");

        public bool IsVariable(string bindingName)
        {
            if (CurrentEnv.TryGetValue(bindingName, out Term? value))
            {
                return value == _variableMarker;
            }
            return false;
        }

        public void MarkVariable(string bindingName)
        {
            CurrentEnv[bindingName] = _variableMarker;
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
