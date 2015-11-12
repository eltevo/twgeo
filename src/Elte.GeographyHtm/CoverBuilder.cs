using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;

namespace Elte.GeographyHtm
{
    public class CoverBuilder
    {
        private SqlGeography geo;

        private int minLevel;
        private int maxLevel;
        private int currentLevel;
        private int maxSteps;
        private int currentStep;

        private Queue<Trixel> trixelQueue;
        private List<Trixel> innerList;
        private List<Trixel> partialList;

        private UInt64 queueArea;
        private UInt64 innerArea;
        private UInt64 partialArea;

        public CoverBuilder(SqlGeography geo)
        {
            InizializeMembers();

            this.geo = geo;
        }

        private void InizializeMembers()
        {
            this.minLevel = -1;
            this.maxLevel = Constants.HtmCoverMaxLevel;
            this.currentLevel = -1;

            this.geo = null;
        }

        public void Execute()
        {
            InitializeBuild();

            do
            {
                Step();

                // All remaining trixels in the queue are partial
                partialList.AddRange(trixelQueue);

                currentStep++;
            }
            while (EvaluateStopCriteria());
        }

        private void Step()
        {
            var trixel = trixelQueue.Dequeue();
            var triangle = GetTriangle(trixel);

            queueArea -= trixel.Area;
            currentLevel = trixel.Level;

            if (geo.STContains(triangle))
            {
                // Inner

                innerList.Add(trixel);
                innerArea += trixel.Area;
            }
            else if (geo.STIntersects(triangle))
            {
                // Partial

                if (currentLevel < maxLevel)
                {
                    var v = trixel.Expand();
                    for (int i = 0; i < v.Length; i++)
                    {
                        trixelQueue.Enqueue(v[i]);
                        queueArea += v[i].Area;
                    }
                }
                else
                {
                    partialList.Add(trixel);
                    partialArea += trixel.Area;
                }
            }
            else
            {
                // Outer
            }
        }

        private void InitializeBuild()
        {
            queueArea = 0;
            innerArea = 0;
            partialArea = 0;

            trixelQueue = new Queue<Trixel>();

            // Add initial octahedron
            for (int i = 0; i < Constants.Faces.Length; i++)
            {
                var trixel = Constants.Faces[i].Trixel;
                trixelQueue.Enqueue(trixel);
                queueArea += trixel.Area;
            }

            innerList = new List<Trixel>();
            partialList = new List<Trixel>();
        }

        private bool EvaluateStopCriteria()
        {
            if (trixelQueue.Count == 0)
            {
                return false;
            }

            return true;
        }

        private SqlGeography GetTriangle(Trixel trixel)
        {
            var geoBuilder = new SqlGeographyBuilder();
            geoBuilder.SetSrid(geo.STSrid.Value);

            Point v0, v1, v2;
            trixel.GetCorners(out v0, out v1, out v2);
            

            geoBuilder.BeginGeography(OpenGisGeographyType.Polygon);
            geoBuilder.BeginFigure(v0.Lat, v0.Lon);
            geoBuilder.AddLine(v1.Lat, v1.Lon);
            geoBuilder.AddLine(v2.Lat, v2.Lon);
            geoBuilder.AddLine(v0.Lat, v0.Lon);
            geoBuilder.EndFigure();
            geoBuilder.EndGeography();

            return geoBuilder.ConstructedGeography;
        }

        public Range[] GetRanges()
        {
            var res = new Range[innerList.Count + partialList.Count];

            int i = 0;

            foreach (var t in innerList)
            {
                res[i++] = t.GetRange(maxLevel, Markup.Inner);
            }

            foreach (var t in partialList)
            {
                res[i++] = t.GetRange(maxLevel, Markup.Partial);
            }

            return res;
        }
    }
}
