using System.Collections;

using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.SchemeData
{
    internal sealed class Cons : ISchemeExp, ICons<ISchemeExp>
    {
        public ISchemeExp Car { get; private set; }
        public ISchemeExp Cdr { get; private set; }

        public bool IsAtom => false;
        public bool IsNil => false;

        public Cons(ISchemeExp car, ISchemeExp cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public void SetCar(ISchemeExp car) => Car = car;
        public void SetCdr(ISchemeExp cdr) => Cdr = cdr;

        public override string ToString() => IConsExtensions.ToString(this);
        public void Print(TextWriter writer, int indent) => IConsExtensions.Print(this, writer, indent);

        public IEnumerator<ISchemeExp> GetEnumerator() => this.Enumerate();
        IEnumerator IEnumerable.GetEnumerator() => this.Enumerate();
    }
}
