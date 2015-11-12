using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Elte.GeographyHtm
{
    public static class Htm
    {
        public static UInt64 GetStartPane(Point p)
        {
            if ((p.X > 0) && (p.Y >= 0))
            {
                return (p.Z >= 0) ? Constants.N3 : Constants.S0;
            }
            else if ((p.X <= 0) && (p.Y > 0))
            {
                return (p.Z >= 0) ? Constants.N2 : Constants.S1;
            }
            else if ((p.X < 0) && (p.Y <= 0))
            {
                return (p.Z >= 0) ? Constants.N1 : Constants.S2;
            }
            else if ((p.X >= 0) && (p.Y < 0))
            {
                return (p.Z >= 0) ? Constants.N0 : Constants.S3;
            }
            else
            {
                return (p.Z >= 0) ? Constants.N3 : Constants.S0;
            }
        }

        public static void GetStartPaneVectors(UInt64 htmID, out Point v0, out Point v1, out Point v2)
        {
            int bix = (int)(htmID - 8L);

            v0 = Constants.Faces[bix].Point0;
            v1 = Constants.Faces[bix].Point1;
            v2 = Constants.Faces[bix].Point2;
        }
    }
}
