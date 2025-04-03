using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Static;
using Clasp.Data.Terms;

namespace Clasp.Data.Abstractions.SpecialForms
{
    /// <summary>
    /// Represents a process by which a single <see cref="AbstractProgram"/> among multiple
    /// is chosen to be executed.
    /// </summary>
    class Alternative : AbstractSpecialForm
    {
        public readonly AbstractProgram Conditional;
        public readonly AbstractProgram Consequent;
        public readonly AbstractProgram AntiConsequent;

        public Alternative(AbstractProgram conditional, AbstractProgram consequent, AbstractProgram antiConsequent)
        {
            Conditional = conditional;
            Consequent = consequent;
            AntiConsequent = antiConsequent;
        }

        public override string ToString() => $"{Conditional} ? {Consequent} : {AntiConsequent}";
        public override ITerm Express()
        {
            return Cons.List(Symbols.S_If, Conditional.Express(), Consequent.Express(), AntiConsequent.Express(), new Nil());
        }
    }
}
