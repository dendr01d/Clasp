using System.Collections;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeData
{
    internal sealed class Cons : ISchemeExp, ICons<ISchemeExp>
    {
        public ISchemeExp Car { get; private set; }
        public ISchemeExp Cdr { get; private set; }
        public SchemeType Type { get; }

        public bool IsAtom => false;
        public bool IsNil => false;

        public Cons(ISchemeExp car, ISchemeExp cdr)
        {
            Car = car;
            Cdr = cdr;
            Type = new PairType(car.Type, cdr.Type);
        }

        public void SetCar(ISchemeExp car) => Car = car;
        public void SetCdr(ISchemeExp cdr) => Cdr = cdr;

        bool IPrintable.BreaksLine => Car.BreaksLine || Cdr is ICons<ISchemeExp>;
        public string AsString => this.Stringify(x => x.IsNil);
        public void Print(TextWriter writer, int indent) => writer.WriteCons(this, indent);
        public sealed override string ToString() => AsString;

        public IEnumerator<ISchemeExp> GetEnumerator() => this.Enumerate();
        IEnumerator IEnumerable.GetEnumerator() => this.Enumerate();
    }
}
