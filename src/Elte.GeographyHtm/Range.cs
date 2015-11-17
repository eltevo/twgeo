using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;

namespace Elte.GeographyHtm
{
    public struct Range : IComparable<Range>
    {
        private Trixel lo;
        private Trixel hi;
        private Markup markup;
        private SqlGeography intersection;

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

        public SqlGeography Intersection
        {
            get { return intersection; }
            set { intersection = value; }
        }

        public Range(Trixel lo, Trixel hi)
        {
            this.lo = lo;
            this.hi = hi;
            this.markup = Markup.Undefined;
            this.intersection = null;
        }

        public Range(Trixel lo, Trixel hi, Markup markup)
        {
            this.lo = lo;
            this.hi = hi;
            this.markup = markup;
            this.intersection = null;
        }

        public Range(Range range, Markup markup)
        {
            this.lo = range.lo;
            this.hi = range.hi;
            this.markup = markup;
            this.intersection = range.intersection;
        }

        public static Range Null
        {
            get
            {
                return new Range(Trixel.Null, Trixel.Null);
            }
        }

        public static bool operator ==(Range range, Range other)
        {
            return range.lo == other.lo && range.hi == other.hi;
        }

        public static bool operator !=(Range range, Range other)
        {
            return range.lo != other.lo || range.hi != other.hi;
        }

        public bool IsDisjoint(Range b)
        {
            return (this.hi < b.lo || b.hi < this.lo);
        }

        public override bool Equals(object obj)
        {
            var r = (Range)obj;
            return this.lo == r.lo && this.hi == r.hi;
        }

        public override int GetHashCode()
        {
            return this.lo.GetHashCode() ^ this.hi.GetHashCode();
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
