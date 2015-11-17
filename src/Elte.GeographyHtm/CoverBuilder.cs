using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;

namespace Elte.GeographyHtm
{
    public class CoverBuilder
    {
        private enum WorkerState
        {
            Work,
            Wait,
            Quit,
        }

        private SqlGeography geo;

        private int threads;
        private int minLevel;
        private int maxLevel;
        private int maxSteps;
        private bool merge;

        private ConcurrentDictionary<UInt64, SqlGeography> geoCache;
        private Queue<Trixel> trixelQueue;
        private List<Trixel> innerList;
        private List<Trixel> partialList;

        private CountdownEvent counter;
        private UInt64 working;

        private UInt64 queueArea;
        private UInt64 innerArea;
        private UInt64 partialArea;

        public int Threads
        {
            get { return threads; }
            set { threads = value; }
        }

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

            this.threads = Math.Min(64, Math.Max(1, Environment.ProcessorCount / 2));
            this.minLevel = -1;
            this.maxLevel = Constants.HtmCoverMaxLevel;
            this.maxSteps = -1;
            this.merge = true;
        }

        private void InitializeBuild()
        {
            queueArea = 0;
            innerArea = 0;
            partialArea = 0;

            geoCache = new ConcurrentDictionary<UInt64, SqlGeography>();
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

            counter = new CountdownEvent(threads);
            working = 0UL;

            for (int i = 0; i < threads; i++)
            {
                var thread = new Thread(Worker);
                thread.Start(i);
            }

            counter.Wait();
            counter.Dispose();
        }

        private void Worker(object threadState)
        {
            int workerId = (int)threadState;

            while (true)
            {
                Trixel trixel = Trixel.Null;
                WorkerState state;

                // Try to take an item or check if all threads are finished
                lock (trixelQueue)
                {
                    if (trixelQueue.Count > 0)
                    {
                        // There's work to do
                        trixel = trixelQueue.Dequeue();

                        working |= (1UL << workerId);
                        state = WorkerState.Work;
                        queueArea -= trixel.Area;
                    }
                    else if (working == 0)
                    {
                        // All threads have finished
                        state = WorkerState.Quit;
                    }
                    else
                    {
                        // There are working threads
                        state = WorkerState.Wait;
                    }
                }

                if (state == WorkerState.Quit)
                {
                    // Signal event and quit
                    counter.Signal();
                    break;
                }
                else if (state == WorkerState.Wait)
                {
                    // Spin wait for the outcome of other threads
                    Thread.SpinWait(100);
                    continue;
                }

                var g = GetParentGeo(trixel);
                var triangle = trixel.GetTriangle(g);

                if (g.Filter(triangle))
                {
                    // Compute intersection
                    var intersection = g.STIntersection(triangle);
                    var area = Math.Abs(intersection.STArea().Value - triangle.STArea().Value);

                    if (area < triangle.STArea().Value * 1e-7)
                    {
                        // Inner, whole triangle is inside
                        lock (innerList)
                        {
                            innerList.Add(trixel);
                            innerArea += trixel.Area;
                        }
                    }
                    else
                    {
                        // Partial, store intersection in cache
                        // this won't fail because HtmID is unique
                        geoCache.TryAdd(trixel.HtmID, intersection);

                        if (trixel.Level < maxLevel)
                        {
                            var v = trixel.Split();

                            lock (trixelQueue)
                            {
                                for (int i = 0; i < v.Length; i++)
                                {
                                    trixelQueue.Enqueue(v[i]);
                                    queueArea += v[i].Area;
                                }
                            }
                        }
                        else
                        {
                            lock (partialList)
                            {
                                partialList.Add(trixel);
                                partialArea += trixel.Area;
                            }

                        }
                    }
                }

                // Outer, do nothing

                lock (trixelQueue)
                {
                    working &= ~(1UL << workerId);
                }
            }
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
