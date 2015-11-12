using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elte.GeographyHtm
{
    public struct Point
    {
        #region Privatemember variables

        private double x, y, z;
        private double lon, lat;

        #endregion
        #region Public properties

        public double X
        {
            get { return x; }
            set { x = value; }
        }

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public double Z
        {
            get { return z; }
            set { z = value; }
        }

        public double Norm
        {
            get
            {
                if (IsNan)
                {
                    return double.NaN;
                }
                else
                {
                    return Math.Sqrt(this.Norm2);
                }
            }
        }

        public double Norm2
        {
            get
            {
                if (IsNan)
                {
                    return double.NaN;
                }
                else
                {
                    return X * X + Y * Y + Z * Z;
                }
            }
        }

        public double Lon
        {
            get
            {
                if (double.IsNaN(lon))
                {
                    UpdateLon();
                }

                return lon;
            }
        }

        public double Lat
        {
            get
            {
                if (double.IsNaN(lat))
                {
                    UpdateLat();
                }

                return lat;
            }
        }

        public bool IsNan
        {
            get { return double.IsNaN(this.x) && double.IsNaN(this.lon); }
        }

        #endregion
        #region Constructors and initializers

        public Point(double x, double y, double z, bool normalize)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.lon = double.NaN;
            this.lat = double.NaN;

            if (normalize)
            {
                Normalize();
            }
        }

        public Point(double lon, double lat)
        {
            this.x = double.NaN;
            this.y = double.NaN;
            this.z = double.NaN;
            this.lon = lon;
            this.lat = lat;
        }

        public static Point NaN
        {
            get
            {
                return new Point()
                {
                    x = double.NaN,
                    y = double.NaN,
                    z = double.NaN,
                    lon = double.NaN,
                    lat = double.NaN,
                };
            }
        }

        #endregion
        #region Operations

        public void UpdateLat()
        {
            if (double.IsNaN(z))
            {
                lat = double.NaN;
            }
            else if (Math.Abs(z) < 1.0)
            {
                lat = Math.Asin(z) * Constants.RadianToDegree;
            }
            else
            {
                lat = 90.0 * Math.Sign(z);
            }
        }

        public void UpdateLon()
        {
            if (double.IsNaN(x) || double.IsNaN(y))
            {
                lon = double.NaN;
            }
            else
            {
                lon = Math.Atan2(y, x) * Constants.RadianToDegree;
            }
        }

        public void Normalize()
        {
            var norm = Norm;

            x /= norm;
            y /= norm;
            z /= norm;
        }

        #endregion
        #region Operators

        public static double operator *(Point a, Point b)
        {
            return Dot(a, b);
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y, a.Z + b.Z, false);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y, a.Z - b.Z, false);
        }

        #endregion
        #region Geometrical relations

        public static double Dot(Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Point Cross(Point a, Point b)
        {
            return new Point(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X,
                false);
        }

        public static double Triple(Point p1, Point p2, Point p3)
        {
            double x, y, z;

            x = p1.y * p2.z - p1.z * p2.y;
            y = p1.z * p2.x - p1.x * p2.z;
            z = p1.x * p2.y - p1.y * p2.x;

            return x * p3.x + y * p3.y + z * p3.z;
        }

        public static Point MidPoint(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y, a.Z + b.Z, true);
        }

        public static Point MidPoint(Point a, Point b, Point c)
        {
            return new Point(a.X + b.X + c.X, a.Y + b.Y + c.Y, a.Z + b.Z + c.Z, true);
        }

        public static Point MidPoint(IList<Point> points)
        {
            double x = 0;
            double y = 0;
            double z = 0;

            foreach (Point p in points)
            {
                x += p.x;
                y += p.y;
                z += p.z;
            }
            
            return new Point(x, y, z, true);
        }

        public static double Distance2(Point a, Point b)
        {
            return (a - b).Norm2;
        }

        public static double Distance(Point a, Point b)
        {
            return Math.Sqrt(Distance2(a, b));
        }

        public static double AngleInRadian(Point a, Point b)
        {
            var d = 0.5 * Distance(a, b);

            if (d < 1)
            {
                return 2 * Math.Asin(d);
            }
            else
            {
                return Math.PI;
            }
        }

        public static double AngleInDegree(Point a, Point b)
        {
            return AngleInRadian(a, b) * Constants.RadianToDegree;
        }

        public static double AngleInArcmin(Point a, Point b)
        {
            return AngleInRadian(a, b) * Constants.RadianToArcmin;
        }

        public bool IsInside(Point v0, Point v1, Point v2)
        {
            if (Triple(v0, v1, this) < -Constants.DoublePrecision2x)
            {
                return false;
            }

            if (Triple(v1, v2, this) < -Constants.DoublePrecision2x)
            {
                return false;
            }

            if (Triple(v2, v0, this) < -Constants.DoublePrecision2x)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
