﻿using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCps.Abstract;

namespace ClaspCompiler.IntermediateCps
{
    //internal sealed class Conditional : ITail
    //{
    //    public string ControlCode => "IFF";

    //    public ICpsExp Condition { get; init; }
    //    public ITail Consequent { get; init; }
    //    public ITail Alternative { get; init; }

    //    public Dictionary<VarBase, int> FreeVariables { get; init; }

    //    public Conditional(ICpsExp cond, ITail consq, ITail alt)
    //    {
    //        Condition = cond;
    //        Consequent = consq;
    //        Alternative = alt;

    //        FreeVariables = cond.CountFreeVariables();
    //    }

    //    public bool BreaksLine => true;
    //    public string AsString => $"({ControlCode} {Condition} {Consequent} {Alternative})";
    //    public void Print(TextWriter writer, int indent)
    //    {
    //        writer.WriteIndenting($"({ControlCode} ", ref indent);
    //        writer.WriteLineIndent(Condition, indent);
    //        writer.WriteLineIndent(Consequent, indent);
    //        writer.Write(Alternative, indent);
    //        writer.Write(')');
    //    }
    //    public sealed override string ToString() => AsString;
    //}
}
