using Clasp.Data.Static;

namespace Clasp.Data.Metadata
{
    internal enum ExpansionMode
    {
        /// <summary>In this mode, the expander expects all forms to be expressions and allows no imperative forms.</summary>
        Expression,
        /// <summary>Represents the expansion of a sequence body as part of a <see cref="Keywords.BEGIN"/> or <see cref="Keywords.LAMBDA"/> form.</summary>
        Sequential,
        /// <summary>Represents the expansion of a sequence body of a <see cref="Keywords.MODULE"/> form.</summary>
        Module,
        /// <summary>Represents the expansion of forms at the top (REPL) level.</summary>
        TopLevel,
        /// <summary>Represents the secondary expansion of a sequence body that was previously partially-expanded.</summary>
        Partial
    }
}
