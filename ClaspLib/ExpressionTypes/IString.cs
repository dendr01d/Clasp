using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib
{
    public interface IString : IComposite
    {
        public int Length { get; }

        public ICharacter AtIndex(int i);

        public void SetIndex(int i, ICharacter value);
    }
}
