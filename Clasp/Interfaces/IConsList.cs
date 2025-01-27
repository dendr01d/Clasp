using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Interfaces
{
    internal interface IConsList<T> : IEnumerable<T?>
        where T : class
    {
        public T Car { get; }

        public T CdrValue { get; }
        public IConsList<T>? CdrList { get; }


        [MemberNotNullWhen(true, nameof(CdrValue))]
        [MemberNotNullWhen(false, nameof(CdrList))]
        public bool IsDotted { get; }


        [MemberNotNullWhen(true, nameof(CdrList))]
        [MemberNotNullWhen(false, nameof(CdrValue))]
        public bool IsList { get; }

        public T Terminator { get; }
    }

    internal interface IStrongConsList<T1, T2> : IEnumerable<T1>
        where T2 : T1, IStrongConsList<T1, T2>
        where T1 : class
    {
        public T1 Car { get; }
        public T2? Cdr { get; }
    }
}
