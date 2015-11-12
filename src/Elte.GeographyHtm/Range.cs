using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elte.GeographyHtm
{
    public struct Range : IComparable<Range>
    {
        private Trixel lo;
        private Trixel hi;
        private Markup markup;

        public Trixel Lo
        {
            get { return lo; }
            set { lo = value; }
        }

        public Trixel Hi
        {
            get { return hi; }
            set { hi = value; }
        }

        public Markup Markup
        {
            get { return markup; }
            set { markup = value; }
        }

        public Range(Trixel lo, Trixel hi)
        {
            this.lo = lo;
            this.hi = hi;
            this.markup = Markup.Undefined;
        }

        public Range(Trixel lo, Trixel hi, Markup markup)
        {
            this.lo = lo;
            this.hi = hi;
            this.markup = markup;
        }

        public Range(Range range, Markup markup)
        {
            this.lo = range.lo;
            this.hi = range.hi;
            this.markup = markup;
        }

        public bool IsDisjoint(Range b)
        {
            return (this.hi < b.lo || b.hi < this.lo);
        }

        public int CompareTo(Range b)
        {
            return this.lo.CompareTo(b.lo);
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", this.lo, this.hi, this.markup);
        }
    }
}
