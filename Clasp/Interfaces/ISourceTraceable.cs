﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clasp.Data.Text;

namespace Clasp
{
    public interface ISourceTraceable
    {
        public Token? SourceTrace { get; }
    }
}