using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Static;
using System.Numerics;
using System.Diagnostics;

namespace Clasp.Data.Metadata
{
    /// <summary>
    /// Manages state information of an ongoing program expansion\parse
    /// </summary>
    [DebuggerDisplay("{Mode} @ Phase {Phase}")]
    internal sealed class CompilationContext
    {
        /// <summary>
        /// Records compile-time (i.e. meta-runtime) definitions, indexed by expansion phase.
        /// </summary>
        private readonly Dictionary<int, RootEnv> _metaEnvironments;

        /// <summary>
        /// Represents compile-time definitions strictly available to the current lexical scope being expanded.
        /// </summary>
        /// <remarks>
        /// This is necessarily a <see cref="Closure"/> of the <see cref="RootEnv"/> in <see cref="_metaEnvironments"/>
        /// indexed at <see cref="Phase"/>.
        /// </remarks>
        public MutableEnv CompileTimeEnv;

        /// <summary>
        /// The current phase of expansion.
        /// </summary>
        public readonly int Phase;

        /// <summary>
        /// Holds identifiers that need to be aggregated over the lifetime of this particular context instance.
        /// For example, partially-defined keys in a lambda body, or exported keys in a module.
        /// </summary>
        public readonly List<Identifier> CollectedIdentifiers;

        /// <summary>
        /// Holds identifiers that will be exported from the body of the module this instance presumably covers.
        /// </summary>
        public readonly List<Identifier> ExportedIdentifiers;

        /// <summary>
        /// Holds scopes imported from other modules within this context.
        /// </summary>
        public readonly List<Scope> ImportedScopes;

        /// <summary>
        /// The inside edge of the most immediate scoped sequential form, if one exists.
        /// </summary>
        /// <remarks>
        /// The sequential form in question could represent either a <see cref="Keywords.LAMBDA"/> or <see cref="Keywords.MODULE"/> body.
        /// </remarks>
        public readonly Scope? InsideEdge;

        /// <summary>
        /// The use-site scope of the macro currently being expanded, if it exists.
        /// </summary>
        public readonly Scope? MacroUseSite;

        /// <summary>
        /// Represents the syntactic context of the form currently being expanded, and thus
        /// informs additional rules for how that expansion should proceed.
        /// </summary>
        public readonly ExpansionMode Mode;

        #region Constructors

        private CompilationContext(Dictionary<int, RootEnv> meta, MutableEnv env, int phase, Scope? edge, Scope? site, ExpansionMode mode)
        {
            _metaEnvironments = meta;
            CompileTimeEnv = env;
            Phase = phase;
            CollectedIdentifiers = new List<Identifier>();
            ExportedIdentifiers = new List<Identifier>();
            ImportedScopes = new List<Scope>();
            InsideEdge = edge;
            MacroUseSite = site;
            Mode = mode;
        }

        private static CompilationContext BuildNew(ExpansionMode mode, Scope? insideEdge = null)
        {
            RootEnv phase1Env = new RootEnv();
            Dictionary<int, RootEnv> meta = new Dictionary<int, RootEnv>() { { 1, phase1Env } };

            return new CompilationContext(meta, phase1Env, 1, insideEdge, null, mode);
        }

        public static CompilationContext ForTopLevel() => BuildNew(ExpansionMode.TopLevel);

        public static CompilationContext ForModule(Scope insideEdge) => BuildNew(ExpansionMode.Module, insideEdge);

        /// <summary>
        /// Contextualize a <see cref="Keywords.LAMBDA"/> body form within the current context,
        /// characterized by the given <paramref name="insideEdge"/>.
        /// </summary>
        public CompilationContext InLambdaBody(Scope insideEdge)
        {
            return new CompilationContext(
                meta: _metaEnvironments,
                env: CompileTimeEnv.Enclose(),
                phase: Phase,
                edge: insideEdge,
                site: null,
                mode: ExpansionMode.Sequential);
        }

        //public CompilationContext InModuleBody(Scope insideEdge)
        //{
        //    return new CompilationContext(
        //        meta: _metaEnvironments,
        //        env: CompileTimeEnv.Enclose(),
        //        phase: Phase,
        //        edge: insideEdge,
        //        site: null,
        //        mode: ExpansionMode.Module);
        //}

        /// <summary>
        /// Contextualize a form that is the result of a macro transformation,
        /// characterized by the given <paramref name="macroUseSite"/>.
        /// </summary>
        public CompilationContext InTransformed(Scope macroUseSite)
        {
            return new CompilationContext(
                meta: _metaEnvironments,
                env: CompileTimeEnv,
                phase: Phase,
                edge: InsideEdge,
                site: macroUseSite,
                mode: Mode);
        }

        /// <summary>
        /// Contextualize the meta-expansion of a form in the next phase up from the current one.
        /// </summary>
        public CompilationContext InNextPhase()
        {
            if (_metaEnvironments.ContainsKey(Phase + 1))
            {
                _metaEnvironments[Phase + 1] = new RootEnv();
            }

            return new CompilationContext(
                meta: _metaEnvironments,
                env: _metaEnvironments[Phase + 1].Enclose(),
                phase: Phase + 1,
                edge: null,
                site: null,
                mode: ExpansionMode.TopLevel);
        }

        /// <summary>
        /// Contextualize a form that is required to be or expand to an expression form.
        /// </summary>
        public CompilationContext AsExpression()
        {
            return new CompilationContext(
                meta: _metaEnvironments,
                env: CompileTimeEnv,
                phase: Phase,
                edge: InsideEdge,
                site: MacroUseSite,
                mode: ExpansionMode.Expression);
        }

        /// <summary>
        /// Contextualize a form that has already been partially-expanded.
        /// </summary>
        public CompilationContext AsPartial()
        {
            return new CompilationContext(
                meta: _metaEnvironments,
                env: CompileTimeEnv,
                phase: Phase,
                edge: InsideEdge,
                site: MacroUseSite,
                mode: ExpansionMode.Partial);
        }

        #endregion

        #region Scope Adjustment

        /// <summary>
        /// Adjust the scopes of the given identifier by adding the pending <see cref="InsideEdge"/>
        /// and removing the current <see cref="MacroUseSite"/>, should either respectively be present.
        /// </summary>
        public void SanitizeBindingKey(Identifier target)
        {
            if (InsideEdge is Scope insideEdge)
            {
                target.AddScope(Phase, insideEdge);
            }

            if (MacroUseSite is Scope macroUseSite)
            {
                target.RemoveScope(Phase, macroUseSite);
            }
        }

        /// <summary>
        /// Adjust the scopes of the given identifier by adding the pending <see cref="InsideEdge"/>,
        /// if one is present.
        /// </summary>
        public void AddPendingInsideEdge(Syntax target)
        {
            if (InsideEdge is Scope insideEdge)
            {
                target.AddScope(Phase, insideEdge);
            }
        }

        #endregion

        public void CollectIdentifier(Identifier id) => CollectedIdentifiers.Add(id);
        public void ExportIdentifier(Identifier id) => ExportedIdentifiers.Add(id);
        public void ImportScope(Scope scp) => ImportedScopes.Add(scp);

        /// <summary>
        /// Attempt to dereference a <see cref="MacroProcedure"/> defined using <paramref name="binding"/>
        /// in the <see cref="CompileTimeEnv"/> corresponding to the given <paramref name="phase"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> iff the meta-environment at the given phase exists, it contains
        /// a value defined with <paramref name="binding"/>, and that value is a <see cref="MacroProcedure"/>.
        /// </returns>
        public bool TryLookupMacro(RenameBinding binding,
            [NotNullWhen(true)] out MacroProcedure? macro)
        {
            if (binding.BoundType == BindingType.Transformer
                && _metaEnvironments.TryGetValue(Phase, out RootEnv? env)
                && env.TryGetValue(binding.Name, out Term? maybeMacro)
                && maybeMacro is MacroProcedure definitelyMacro)
            {
                macro = definitelyMacro;
                return true;
            }

            macro = null;
            return false;
        }
    }
}
