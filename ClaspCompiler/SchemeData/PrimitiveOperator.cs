using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeData
{
    /// <summary>
    /// Represents a primitive operator for a function executed by the .NET runtime.
    /// </summary>
    internal class PrimitiveOperator : IValue
    {
        public string Name { get; init; }
        public Symbol Symbol { get; init; }
        public bool HasSideEffect { get; init; }

        public SchemeType Type { get; init; }
        public bool IsAtom => true;
        public bool IsNil => false;
        public bool IsFalse => false;

        public bool BreaksLine => false;
        public string AsString => $"<{Name}>";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;


        public PrimitiveOperator(string name, SchemeType type, bool hasSideEffect)
        {
            Name = name;
            Symbol = SymbolFactory.InternGlobal(name);

            Type = type;
            HasSideEffect = hasSideEffect;
        }

        //private static PrimitiveOperator Init(string name, SchemeType type, bool hasSideEffect = false)
        //{
        //    PrimitiveOperator op = new(name, type, hasSideEffect);

        //    DefaultBindings.AddPrimitive(op);

        //    return op;
        //}

    }
}