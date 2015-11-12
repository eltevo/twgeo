using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elte.GeographyHtm
{
    static class Constants
    {
        public static readonly double DoublePrecision = Math.Pow(2, -53);
        public static readonly double DoublePrecision2x = 2 * DoublePrecision;
        public static readonly double DoublePrecision3x = 3 * DoublePrecision;
        public static readonly double DoublePrecision4x = 4 * DoublePrecision;

        public const double RadianToDegree = 57.295779513082320876798154814105;
        public const double RadianToArcmin = 3437.7467707849392526078892888463;

        public const int HtmIDBits = 64;
        public const int HtmLevel = 20;
        public const UInt64 HtmIDHighBit1 = 0x2000000000000000UL;
        public const UInt64 HtmIDHighBit2 = 0x1000000000000000UL;
        public const int HtmIDMaxNameLength = 32;

        public const int HtmCoverMaxLevel = 8;

        public const UInt64 S0 = 0x0000000000000008UL;
        public const UInt64 S1 = 0x0000000000000009UL;
        public const UInt64 S2 = 0x000000000000000AUL;
        public const UInt64 S3 = 0x000000000000000BUL;
        public const UInt64 N0 = 0x000000000000000CUL;
        public const UInt64 N1 = 0x000000000000000DUL;
        public const UInt64 N2 = 0x000000000000000EUL;
        public const UInt64 N3 = 0x000000000000000FUL;

        public static readonly Face[] Faces =
        {
            new Face(S0, "S0", 1, 5, 2),
            new Face(S1, "S1", 2, 5, 3),
            new Face(S2, "S2", 3, 5, 4),
            new Face(S3, "S3", 4, 5, 1),
            new Face(N0, "N0", 1, 0, 4),
            new Face(N1, "N1", 4, 0, 3),
            new Face(N2, "N2", 3, 0, 2),
            new Face(N3, "N3", 2, 0, 1),
        };

        public static readonly Point[] OriginalPoints = 
        {
            new Point(0.0, 0.0, 1.0, false),
            new Point(1.0, 0.0, 0.0, false),
            new Point(0.0, 1.0, 0.0, false),
            new Point(-1.0, 0.0, 0.0, false),
            new Point(0.0, -1.0, 0.0, false),
            new Point(0.0, 0.0, -1.0, false),
        };
    }
}
