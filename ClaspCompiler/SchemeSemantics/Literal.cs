﻿using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Literal(IValue Value, uint AstId) : ISemLiteral
    {
        public SchemeType Type => Value.Type;

        public bool BreaksLine => Value.BreaksLine;
        public string AsString => Value.AsString;
        public void Print(TextWriter writer, int indent) => Value.Print(writer, indent);
        public sealed override string ToString() => AsString;
    }
}
