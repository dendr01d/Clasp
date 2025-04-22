namespace VirtualMachine.Objects
{
    /// <summary>
    /// Represents
    /// </summary>
    internal class Cons
    {
        public Term Car { get; private set; }
        public Term Cdr { get; private set; }

        public Cons(Term car, Term cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public void SetCar(Term newCar) => Car = newCar;
        public void SetCdr(Term newCdr) => Cdr = newCdr;
    }
}
