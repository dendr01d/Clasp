﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Metadata;
using Clasp.Data.Text;

namespace Clasp
{
    /// <summary>
    /// Indicates that this class can trace its origin back to a <see cref="SourceLocation"/>.
    /// </summary>
    public interface ISourceTraceable
    {
        public SourceLocation Location { get; }
    }
}
