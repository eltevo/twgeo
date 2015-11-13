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

        private Dictionary<UInt64, SqlGeography> geoCache;
        private Queue<Trixel> trixelQueue;
        private List<Trixel> innerList;
        private List<Trixel> partialList;

        private UInt64 queueArea;
        private UInt64 innerArea;
        private UInt64 partialArea;

        public int MinLevel
        {
            get { return minLevel; }
            set { minLevel = value; }
        }

        public int MaxLevel
        {
            get { return maxLevel; }
            set { maxLevel = value; }
        }

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

        private void InitializeBuild()
        {
            queueArea = 0;
            innerArea = 0;
            partialArea = 0;

            geoCache = new Dictionary<ulong, SqlGeography>();
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

        public void Execute()
        {
            InitializeBuild();

            do
            {
                Step();

                currentStep++;
            }
            while (EvaluateStopCriteria());

            // All remaining trixels in the queue are partial
            partialList.AddRange(trixelQueue);
        }

        private void Step()
        {
            var trixel = trixelQueue.Dequeue();
            var g = GetParentGeo(trixel);
            var triangle = trixel.GetTriangle(g);
            
            queueArea -= trixel.Area;
            currentLevel = trixel.Level;

            if (g.Filter(triangle))
            {
                if (g.STContains(triangle))
                {
                    // Inner, whole triangle is inside
                    innerList.Add(trixel);
                    innerArea += trixel.Area;
                }
                else
                {
                    // Partial, compute intersection and store in cache
                    var intersection = g.STIntersection(triangle);
                    geoCache.Add(trixel.HtmID, intersection);

                    if (currentLevel < maxLevel)
                    {
                        var v = trixel.Split();
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
            }

            // Outer, do nothing
        }

        private SqlGeography GetParentGeo(Trixel trixel)
        {
            if (geoCache.ContainsKey(trixel.Parent.HtmID))
            {
                return geoCache[trixel.Parent.HtmID];
            }
            else
            {
                return geo;
            }
        }

        private bool EvaluateStopCriteria()
        {
            if (trixelQueue.Count == 0)
            {
                return false;
            }

            return true;
        }

        public Range[] GetRanges(bool includeIntersection)
        {
            var res = new Range[innerList.Count + partialList.Count];

            int i = 0;

            foreach (var t in innerList)
            {
                res[i++] = t.GetRange(Constants.HtmLevel, Markup.Inner);
            }

            foreach (var t in partialList)
            {
                var r = t.GetRange(Constants.HtmLevel, Markup.Partial);

                if (includeIntersection)
                {
                    var g = GetParentGeo(t);
                    r.Intersection = g.STIntersection(t.GetTriangle(g));
                }

                res[i++] = r;
            }

            return res;
        }
    }
}
