using System;

using Clasp.Data.Abstractions;

namespace Clasp.Data.Terms
{
    internal interface ITerm : IAbstractForm, IEquatable<ITerm>
    {
    }
}
