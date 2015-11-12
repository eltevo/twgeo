using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elte.GeographyHtm
{
    class SmartTrixel
    {
        private Trixel trixel;
        private int level;
        private UInt64 pseudoArea;
        private Point[] corners;

        public Trixel Trixel
        {
            get { return trixel; }
        }

        public int Level
        {
            get { return level; }
        }

        public UInt64 PseudoArea
        {
            get { return pseudoArea; }
        }

        public Point[] Corners
        {
            get { return corners; }
        }

        public SmartTrixel(Trixel trixel)
        {
            this.trixel = trixel;
            this.level = trixel.Level;
            this.pseudoArea = trixel.PseudoArea;
            this.corners = trixel.GetCorners();
        }

        public SmartTrixel(Trixel trixel, int level, UInt64 pseudoArea, Point[] corners)
        {
            this.trixel = trixel;
            this.level = level;
            this.pseudoArea = pseudoArea;
            this.corners = corners;
        }

        public SmartTrixel[] Expand()
        {
            var w0 = Point.MidPoint(corners[1], corners[2]);
            var w1 = Point.MidPoint(corners[2], corners[0]);
            var w2 = Point.MidPoint(corners[0], corners[1]);

            int l = level + 1;
            UInt64 a = pseudoArea >> 2;
            UInt64 id = this.trixel << 2;
            

            return new SmartTrixel[]
            {
                new SmartTrixel(id++, l, a, new Point[] { corners[0], w2, w1 }),
                new SmartTrixel(id++, l, a, new Point[] { corners[1], w0, w2 }),
                new SmartTrixel(id++, l, a, new Point[] { corners[2], w1, w0 }),
                new SmartTrixel(id++, l, a, new Point[] { w0, w1, w2 }),
            };
        }
    }
}
