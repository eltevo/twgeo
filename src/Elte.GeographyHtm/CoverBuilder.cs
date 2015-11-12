using System;
using System.Collections.Generic;
using System.Linq;
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

        private Queue<SmartTrixel> trixelQueue;
        private List<SmartTrixel> innerList;
        private List<SmartTrixel> partialList;

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

                currentStep++;
            }
            while (EvaluateStopCriteria());
        }

        private void Step()
        {
            var trixel = trixelQueue.Dequeue();
            var triangle = GetTriangle(trixel);

            queueArea -= trixel.Trixel.PseudoArea;
            currentLevel = trixel.Level;

            if (geo.STContains(triangle))
            {
                // Inner

                innerList.Add(trixel);
                innerArea += trixel.PseudoArea;
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
                        queueArea += v[i].PseudoArea;
                    }
                }
                else
                {
                    partialList.Add(trixel);
                    partialArea += trixel.PseudoArea;
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

            trixelQueue = new Queue<SmartTrixel>();

            // Add initial octahedron
            for (int i = 0; i < Constants.Faces.Length; i++)
            {
                var trixel = Constants.Faces[i].Trixel;
                trixelQueue.Enqueue(new SmartTrixel(trixel));
                queueArea += trixel.PseudoArea;
            }

            innerList = new List<SmartTrixel>();
            partialList = new List<SmartTrixel>();
        }

        private bool EvaluateStopCriteria()
        {
            if (trixelQueue.Count == 0)
            {
                return false;
            }

            return true;
        }

        private SqlGeography GetTriangle(SmartTrixel trixel)
        {
            var geoBuilder = new SqlGeographyBuilder();
            geoBuilder.SetSrid(geo.STSrid.Value);

            var v = trixel.Corners;
            

            geoBuilder.BeginGeography(OpenGisGeographyType.Polygon);
            geoBuilder.BeginFigure(v[0].Lat, v[0].Lon);
            geoBuilder.AddLine(v[1].Lat, v[1].Lon);
            geoBuilder.AddLine(v[2].Lat, v[2].Lon);
            geoBuilder.AddLine(v[0].Lat, v[0].Lon);
            geoBuilder.EndFigure();
            geoBuilder.EndGeography();

            return geoBuilder.ConstructedGeography;
        }
    }
}
