using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlTypes;

namespace Elte.GeographyHtm
{
    public struct Trixel : IComparable<Trixel>
    {
        #region Privatemember variables

        private UInt64 htmID;

        #endregion
        #region Public properties

        public UInt64 HtmID
        {
            get { return htmID; }
            set
            {
                htmID = value;

                ValidateHtmID();
            }
        }

        public bool IsValid
        {
            get
            {
                if (htmID < 0)
                {
                    return false;
                }

                if (htmID < 8)
                {
                    return false;
                }

                return true;
            }
        }

        public int Level
        {
            get
            {
                // Find the highest 1 bit to determine level
                // Skip first two bits to avoid signed/unsigned problems

                for (int i = 2; i < Constants.HtmIDBits; i += 2)
                {
                    if (((htmID << i) & Constants.HtmIDHighBit1) != 0)
                    {
                        return (Constants.HtmIDBits - i - 6) / 2;
                    }
                }

                throw new Exception("Invalid HTMID");   // *** TODO
            }
        }

        public UInt64 PseudoArea
        {
            get
            {
                UInt64 res = 1;

                for (int i = Constants.HtmLevel - Level; i > 0; i--)
                {
                    res <<= 2;
                }

                return res;
            }
        }

        #endregion
        #region Constructors and initializers

        public Trixel(UInt64 htmID)
        {
            this.htmID = htmID;

            ValidateHtmID();
        }

        public Trixel(SqlInt64 htmID)
        {
            this.htmID = (UInt64)htmID.Value;

            ValidateHtmID();
        }

        public Trixel(double lon, double lat)
            :this(new Point(lon, lat))
        {
        }

        public Trixel(double x, double y, double z)
            : this(new Point(x, y, z, true))
        {
        }

        public Trixel(Point p)
        {
            this.htmID = FromPoint(p, Constants.HtmLevel);
        }

        public static Trixel Null
        {
            get { return new Trixel(0UL); }
        }

        #endregion
        #region Operators

        public static implicit operator Trixel(UInt64 htmID)
        {
            return new Trixel(htmID);
        }

        public static implicit operator UInt64(Trixel trixel)
        {
            return trixel.htmID;
        }

        public static implicit operator Trixel(SqlInt64 htmID)
        {
            return new Trixel((UInt64)htmID.Value);
        }

        public static implicit operator SqlInt64(Trixel trixel)
        {
            return new SqlInt64((Int64)trixel.htmID);
        }

        public static implicit operator Trixel(SqlString htmID)
        {
            return Trixel.Parse(htmID.Value);
        }

        public static implicit operator SqlString(Trixel trixel)
        {
            return new SqlString(trixel.ToString());
        }

        public static bool operator ==(Trixel trixel, Trixel other)
        {
            return trixel.htmID == other.htmID;
        }

        public static bool operator !=(Trixel trixel, Trixel other)
        {
            return trixel.htmID != other.htmID;
        }

        public static bool operator ==(Trixel trixel, UInt64 other)
        {
            return trixel.htmID == other;
        }

        public static bool operator !=(Trixel trixel, UInt64 other)
        {
            return trixel.htmID != other;
        }

        #endregion

        public override int GetHashCode()
        {
            return htmID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return htmID.Equals(obj);
        }

        public int CompareTo(Trixel to)
        {
            return htmID.CompareTo(to.htmID);
        }

        private void ValidateHtmID()
        {
            if (!IsValid)
            {
                throw new Exception("Invalid htmID");   // *** TODO
            }
        }

        public bool IsAncestorOf(Trixel other)
        {
            int shifts = Level - other.Level;

            if (shifts < 0)
            {
                return false;
            }

            UInt64 descendant = HtmID >> (shifts * 2);

            return (descendant == other);
        }

        public static UInt64 FromPoint(Point p, int level)
        {
            Point v0, v1, v2;
            Point w0, w1, w2;

            UInt64 htmid = Htm.GetStartPane(p);
            Htm.GetStartPaneVectors(htmid, out v0, out v1, out v2);

            // Start searching for the children
            while (level-- > 0)
            {
                htmid <<= 2;

                w2 = Point.MidPoint(v0, v1);
                w0 = Point.MidPoint(v1, v2);
                w1 = Point.MidPoint(v2, v0);

                if (p.IsInside(v0, w2, w1))
                {
                    htmid |= 0;
                    v1 = w2;
                    v2 = w1;
                }
                else if (p.IsInside(v1, w0, w2))
                {
                    htmid |= 1;
                    v0 = v1;
                    v1 = w0;
                    v2 = w2;
                }
                else if (p.IsInside(v2, w1, w0))
                {
                    htmid |= 2;
                    v0 = v2;
                    v1 = w1;
                    v2 = w0;
                }
                else if (p.IsInside(w0, w1, w2))
                {
                    htmid |= 3;
                    v0 = w0;
                    v1 = w1;
                    v2 = w2;
                }
                else
                {
                    throw new Exception("Panic in Cartesian2hid");
                }
            }

            return htmid;
        }

        public Point GetCenter()
        {
            return Point.MidPoint(GetCorners());
        }

        public Point[] GetCorners()
        {
            Point v0, v1, v2;
            Point w0, w1, w2;
            UInt64 bix;

            int level = Level;

            // First get the base plane from top four bits
            int k;

            // Top two bits determine N or S
            bix = (htmID >> (level * 2 + 2)) & 0x03;

            switch (bix)
            {
                case 0x03:  // N
                    k = 4;
                    break;
                case 0x02:  // S
                    k = 0;
                    break;
                default:
                    throw new Exception("Invalid HTMID");   // *** TODO
            }

            // Next two bits determine N or S quater plane
            bix = (htmID >> (level * 2)) & 0x03;
            k += (int)bix;

            v0 = Constants.Faces[k].Point0;
            v1 = Constants.Faces[k].Point1;
            v2 = Constants.Faces[k].Point2;

            for (int i = 0; i < level; i++)
            {
                bix = (htmID >> ((level - i) * 2 - 2)) & 0x03;

                w2 = Point.MidPoint(v0, v1);
                w0 = Point.MidPoint(v1, v2);
                w1 = Point.MidPoint(v2, v0);

                switch (bix)
                {
                    case 0x00:
                        v1 = w2;
                        v2 = w1;
                        break;
                    case 0x01:
                        v0 = v1;
                        v1 = w0;
                        v2 = w2;
                        break;
                    case 0x02:
                        v0 = v2;
                        v1 = w1;
                        v2 = w0;
                        break;
                    case 0x03:
                        v0 = w0;
                        v1 = w1;
                        v2 = w2;
                        break;
                    default:
                        throw new InvalidOperationException();  // *** TODO
                }
            }

            return new Point[] { v0, v1, v2 };
        }

        public Trixel Truncate(int level)
        {
            return new Trixel((htmID >> 2 * (Level - level)));
        }

        public Range Expand(int targetLevel)
        {
            Trixel lo, hi;

            int level = this.Level;
            int shift = 2 * (targetLevel - level);

            if (targetLevel > level)
            {
                lo = htmID << shift;
                hi = lo + ((0x01UL << shift) - 1);
            }
            else
            {
                lo = htmID >> -shift;
                hi = lo;
            }

            return new Range(lo, hi);
        }

        public static Trixel Parse(string name)
        {
            name = name.Trim().ToUpperInvariant();

            if (name.Length < 2)
            {
                //return 0;	// 0 is an illegal HID
                throw new Exception("Illegal HtmID");   // *** TODO
            }

            if (name.Length > Constants.HtmIDMaxNameLength)
            {
                // return 0;
                throw new Exception("Illegal HtmID");   // *** TODO
            }


            UInt64 tempid;

            // Top bit is always 1
            switch (name[0])
            {
                case 'N':
                    tempid = 0x03;
                    break;
                case 'S':
                    tempid = 0x02;
                    break;
                default:
                    throw new Exception("Illegal HtmID");   // *** TODO
            }

            for (int i = 1; i < name.Length; i++)
            {
                tempid <<= 2;
                tempid |= (UInt16)((name[i] - '0') & 0x03);
            }

            return new Trixel(tempid);
        }

        public override string ToString()
        {
            // HtmID string format:
            // C0123012301230123012301230123
            // Here C is N or S, one number for main plane followed
            // by a digits equal to the level of HTM

            int level = Level;
            char[] name = new char[level + 2];

            UInt64 tempid = htmID;

            for (int i = 0; i < level + 1; i++)
            {
                name[level - i + 1] = (char)('0' + (int)(tempid & 0x03L));
                tempid >>= 2;
            }

            // Now the very last bit determines N or S
            name[0] = ((tempid & 0x01) != 0) ? 'N' : 'S';

            return new String(name);
        }
    }
}
