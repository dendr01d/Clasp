﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Values
{
    internal readonly struct Character : IAbstractValue
    {
        public readonly char Value;

        public Character(char value)
        {
            Value = value;
        }
    }
}
