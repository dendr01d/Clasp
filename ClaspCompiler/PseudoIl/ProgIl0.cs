using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClaspCompiler.Common;

namespace ClaspCompiler.PseudoIl
{
    class ProgIl0
    {
        public readonly Var[] Locals;
        public readonly Dictionary<Label, Block> LabeledBlocks;

        public ProgIl0(Var[] locals, Dictionary<Label, Block> labeledBlocks)
        {
            Locals = locals;
            LabeledBlocks = labeledBlocks;
        }
        private string FormatLocals() => string.Format("({0})", string.Join(' ', Locals.Select(x => x.ToString())));

        public override string ToString() => string.Format("(program {0} ({1}))",
            FormatLocals(),
            string.Join(' ', LabeledBlocks.Select(x => string.Format("({0} . {1})", x.Key, x.Value))));

        public void Print(TextWriter writer, int indent)
        {
            writer.Write("(program ");
            writer.Write(FormatLocals());
            writer.WriteLineIndent(indent);

            //foreach(var labeledBlock in LabeledBlocks)
            //{
            //    writer.Write('(');
            //    writer.Write(labeledBlock.Key);
            //    writer.Write(" .");
            //    writer.WriteLineIndent(indent + 3);
            //    writer.Write(labeledBlock.Value, indent + 3);
            //}

            //writer.Write(')');


            writer.WriteIndenting("  (", ref indent);

            foreach (var pair in LabeledBlocks)
            {
                writer.Write('(');
                writer.Write(pair.Key);
                writer.WriteLineIndent(" .", indent);
                writer.Write(pair.Value, indent);
                writer.Write(')');
            }

            writer.Write("))");
        }
    }
}
