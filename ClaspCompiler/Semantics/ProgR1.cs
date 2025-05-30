﻿namespace ClaspCompiler.Semantics
{
    internal sealed class ProgR1 : IPrintable
    {
        public string Info { get; private set; }
        public ISemExp Body { get; private set; }

        public ProgR1(string info, ISemExp body)
        {
            Info = info;
            Body = body;
        }

        public override string ToString() => $"(program {Info} {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(program ", ref indent);
            writer.WriteLineIndent(Info.ToString(), indent);
            writer.Write(Body, indent);
            writer.Write(')');
        }
    }
}
