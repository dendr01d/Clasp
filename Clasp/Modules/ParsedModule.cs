using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Exceptions;
using Clasp.Process;

namespace Clasp.Modules
{
    class ParsedModule : ProcessedModule
    {
        public readonly SequentialForm ModuleBody;

        private ParsedModule(string name, Identifier[] ids, SequentialForm body) : base(name, ids)
        {
            ModuleBody = body;
        }

        public static ParsedModule Parse(ExpandedModule em)
        {
            CoreForm parsed = Parser.ParseModuleSyntax(em.ExpandedBody);

            if (parsed is not SequentialForm sf)
            {
                throw new ClaspGeneralException("Expanded module '{0}' didn't parse to sequential core form as anticipated.", em.Name);
            }
            else
            {
                return new ParsedModule(em.Name, em.ExportedIds, sf);
            }
        }

    }
}
