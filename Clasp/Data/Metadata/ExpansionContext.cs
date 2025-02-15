using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;

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

        /// <summary>The current phase of expansion.</summary>
        public readonly int Phase;

        /// <summary>
        /// The inside edge of the surrounding <see cref="ExpansionMode.InternalDefinition"/>, if it exists.
        /// </summary>
        public readonly Scope? InsideEdge;
        /// <summary>
        /// The use-site scope of the macro currently being expanded, if it exists.
        /// </summary>
        public readonly Scope? MacroUseSite;

        /// <summary>Informs how certain terms should be expanded.</summary>
        public readonly ExpansionMode Mode;

        private ExpansionContext(Environment env, int phase, Scope? edge, Scope? site, ExpansionMode mode)
        {
            CompileTimeEnv = env;
            Phase = phase;
            InsideEdge = edge;
            MacroUseSite = site;
            Mode = mode;
        }

        public ExpansionContext(Environment env, int phase)
            : this(env, phase, null, null, ExpansionMode.TopLevel)
        { }

        public ExpansionContext InBody(Scope insideEdge)
        {
            return new ExpansionContext(
                env: CompileTimeEnv.Enclose(),
                phase: Phase,
                edge: insideEdge,
                site: null,
                mode: ExpansionMode.InternalDefinition);
        }

        public ExpansionContext InTransformed(Scope macroUseSite)
        {
            return new ExpansionContext(
                env: CompileTimeEnv,
                phase: Phase,
                edge: InsideEdge,
                site: macroUseSite,
                mode: Mode);
        }

        public ExpansionContext InModule()
        {
            return new ExpansionContext(
                env: CompileTimeEnv.Enclose(),
                phase: Phase + 1,
                edge: null,
                site: null,
                mode: ExpansionMode.Module);
        }

        public ExpansionContext InNewPhase()
        {
            return new ExpansionContext(
                env: CompileTimeEnv.GlobalEnv.Enclose(),
                phase: Phase + 1,
                edge: null,
                site: null,
                mode: ExpansionMode.TopLevel);
        }

        public ExpansionContext AsExpression()
        {
            return new ExpansionContext(
                env: CompileTimeEnv,
                phase: Phase,
                edge: InsideEdge,
                site: null,
                mode: ExpansionMode.Expression);
        }

        public void SanitizeIdentifier(Identifier target)
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

        public void AddPendingInsideEdge(Syntax target)
        {
            if (InsideEdge is Scope insideEdge)
            {
                target.AddScope(Phase, insideEdge);
            }
        }
    }
}
