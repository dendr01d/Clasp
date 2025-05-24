using System.Collections;

namespace ClaspCompiler.Data
{
    internal sealed class Cons : ICons<ITerm>
    {
        public ITerm Car { get; private set; }
        public ITerm Cdr { get; private set; }

        public bool IsAtom => false;
        public bool IsNil => false;

        public Cons(ITerm car, ITerm cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public void SetCar(ITerm car) => Car = car;
        public void SetCdr(ITerm cdr) => Cdr = cdr;

        public override string ToString() => IConsExtensions.ToString(this);
        public void Print(TextWriter writer, int indent) => IConsExtensions.Print(this, writer, indent);

        public IEnumerator<ITerm> GetEnumerator() => this.Enumerate();
        IEnumerator IEnumerable.GetEnumerator() => this.Enumerate();
    }
}
