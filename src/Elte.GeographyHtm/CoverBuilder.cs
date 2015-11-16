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
        private int maxSteps;
        private bool merge;

        private int currentLevel;
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

        public bool Merge
        {
            get { return merge; }
            set { merge = value; }
        }

        public CoverBuilder(SqlGeography geo)
        {
            InizializeMembers();

            this.geo = geo;
        }

        private void InizializeMembers()
        {
            this.geo = null;

            this.minLevel = -1;
            this.maxLevel = Constants.HtmCoverMaxLevel;
            this.maxSteps = -1;
            this.merge = true;

            this.currentLevel = -1;
            this.currentStep = -1;
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
                // Compute intersection
                var intersection = g.STIntersection(triangle);
                var area = Math.Abs(intersection.STArea().Value - triangle.STArea().Value);

                if (area < triangle.STArea().Value * 1e-7)
                {
                    // Inner, whole triangle is inside
                    innerList.Add(trixel);
                    innerArea += trixel.Area;
                }
                else
                {
                    // Partial, store intersection in cache
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
            var res = new List<Range>(innerList.Count + partialList.Count);
            var prev = Range.Null;

            if (merge)
            {
                innerList.Sort();
            }

            foreach (var t in innerList)
            {
                var r = t.GetRange(Constants.HtmLevel, Markup.Inner);

                // Inner regions are merged, if requested
                if (merge)
                {
                    if (prev == Range.Null)
                    {
                        prev = r;
                    }
                    else if (prev.Hi == r.Lo - 1)
                    {
                        prev.Hi = r.Hi;
                    }
                    else
                    {
                        res.Add(prev);
                        prev = r;
                    }
                }
                else
                {
                    res.Add(r);
                }
            }

            // Append the very last
            if (prev != Range.Null)
            {
                res.Add(prev);
            }

            foreach (var t in partialList)
            {
                var r = t.GetRange(Constants.HtmLevel, Markup.Partial);

                if (includeIntersection)
                {
                    var g = GetParentGeo(t);
                    r.Intersection = g.STIntersection(t.GetTriangle(g));
                }

                res.Add(r);
            }

            return res.ToArray();
        }
    }
}
