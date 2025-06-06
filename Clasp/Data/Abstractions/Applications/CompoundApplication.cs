﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Abstractions.Arguments;
using Clasp.Data.Abstractions.SpecialForms;

namespace Clasp.Data.Abstractions.Applications
{
    /// <summary>
    /// Represents the procedural execution of a <see cref="Function"/>.
    /// </summary>
    internal sealed class CompoundApplication : AbstractApplication
    {
        public readonly Function Operator;

        public CompoundApplication(Function op, AbstractArgument arguments)
            : base(arguments)
        {
            Operator = op;
        }
        public override string Express()
        {
            return $"({Operator.Express()}{Arguments.Express()})";
        }
    }
}
