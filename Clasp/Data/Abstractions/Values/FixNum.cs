﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Values
{
    /// <inheritdoc cref="long"/>
    internal readonly struct FixNum : IAbstractValue
    {
        public readonly long Value;
    }
}
