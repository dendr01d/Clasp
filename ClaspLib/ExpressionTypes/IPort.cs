﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib
{
    public interface IPort : IAtom
    {
        public void Push(string value);
        public string Pull();
    }
}