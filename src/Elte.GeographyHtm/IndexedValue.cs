using System;
using System.Collections.Generic;
using System.Text;

namespace Elte.GeographyHtm
{
    public struct IndexedValue<T>
    {
        public int Index;
        public T Value;

        public IndexedValue(int index, T value)
        {
            this.Index = index;
            this.Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
