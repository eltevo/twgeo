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
            var triangle = trixel.GetTriangle(geo);

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

        public Range[] GetRanges()
        {
            var res = new Range[innerList.Count + partialList.Count];

            int i = 0;

            foreach (var t in innerList)
            {
                res[i++] = t.GetRange(Constants.HtmLevel, Markup.Inner);
            }

            foreach (var t in partialList)
            {
                res[i++] = t.GetRange(Constants.HtmLevel, Markup.Partial);
            }

            return res;
        }
    }
}
