using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elte.GeographyHtm
{
    internal struct Face
    {
        private Trixel trixel;
        private int i0;
        private int i1;
        private int i2;
        private string name;

        public Trixel Trixel
        {
            get { return trixel; }
        }

        public int I0
        {
            get { return i0; }
        }

        public int I1
        {
            get { return i1; }
        }

        public int I2
        {
            get { return i2; }
        }

        public Point Point0
        {
            get { return Constants.OriginalPoints[i0]; }
        }

        public Point Point1
        {
            get { return Constants.OriginalPoints[i1]; }
        }

        public Point Point2
        {
            get { return Constants.OriginalPoints[i2]; }
        }

        public string Name
        {
            get { return name; }
        }

        public Face(Trixel trixel, string name, int i0, int i1, int i2)
        {
            this.trixel = trixel;
            this.i0 = i0;
            this.i1 = i1;
            this.i2 = i2;
            this.name = name;
        }
    }
}
