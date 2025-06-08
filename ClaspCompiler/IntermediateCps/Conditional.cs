using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.IntermediateCps
{
    internal sealed class Conditional : ITail
    {
        public string ControlCode => "IFF";

        public ICpsExp Condition { get; init; }
        public GoTo Consequent { get; init; }
        public GoTo Alternative { get; init; }

        public Dictionary<Var, int> FreeVariables { get; init; }

        public Conditional(ICpsExp cond, GoTo consq, GoTo alt)
        {
            Condition = cond;
            Consequent = consq;
            Alternative = alt;

            FreeVariables = cond.CountFreeVariables();
        }

        public bool BreaksLine => true;
        public string AsString => $"({ControlCode} {Condition} {Consequent} {Alternative})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting($"({ControlCode} ", ref indent);
            writer.WriteLineIndent(Condition, indent);
            writer.WriteLineIndent(Consequent, indent);
            writer.Write(Alternative, indent);
            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}
