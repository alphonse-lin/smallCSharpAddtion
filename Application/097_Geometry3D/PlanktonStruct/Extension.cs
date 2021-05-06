using System;
using System.Collections.Generic;
using g3;
using UrbanX.Application.Geometry;
using System.Linq;

namespace Urbanx.Application.Geometry.Extension
{
    public static class Extension
    {
        public static Vector3d Tog3Pt(this PlanktonVertex ptIn)
        {
            return new Vector3d(ptIn.X, ptIn.Y, ptIn.Z);
        }

        public static Vector3d ClosestPoint(this DCurve3 curve, Vector3d pt, out int crvIndex, out double crvIntr)
        {
            int segCount = curve.SegmentCount;
            Vector3d closestPt = new Vector3d();

            double minCrvDis = double.MaxValue;
            double minCrvIntr = -1;
            int minCrvIndex = -1;

            //Segment3d minSeg = new Segment3d();
            //double tempCrvLength = 0;

            List<double> allLength = new List<double>(segCount);

            for (int i = 0; i < segCount; i++)
            {
                var segment = curve.GetSegment(i);

                var tempPt = segment.ClosestPoint(pt, out double tempCrvIntr, out double distanceSquared3D);
                //allLength.Add(distanceSquared2D);

                if (distanceSquared3D <= minCrvDis)
                {
                    closestPt = tempPt;
                    minCrvDis = distanceSquared3D;
                    minCrvIntr = tempCrvIntr;
                    minCrvIndex = i;
                    //minSeg = segment;
                }
            }

            //for (int i = 0; i < minCrvIndex; i++)
            //    tempCrvLength += allLength[i];
            //crvIntr = (minCrvIntr * minSeg.Length+ tempCrvLength) / allLength.Sum();

            crvIndex = minCrvIndex;
            crvIntr = minCrvIntr;

            return closestPt;
        }

        private static Vector3d ClosestPoint(this Segment3d curve, Vector3d pt, out double crvIntr, out double distanceSquared3D)
        {
            Vector3d p1 = curve.P0;
            Vector3d p2 = curve.P0;

            double dx = p2.x - p1.x;
            double dy = p2.y - p1.y;
            double dz = p2.z - p1.z;

            Vector3d intrPt = new Vector3d();
            if ((dx == 0) && (dy == 0) && (dz == 0))
            {
                // It's a point not a line segment.
                intrPt = p1;
                crvIntr = 0;
                //distanceSquared2D = 0;
                distanceSquared3D = 0;
                return intrPt;
            }

            // Calculate the t that minimizes the distance.
            double t = ((pt.x - p1.x) * dx + (pt.y - p1.y) * dy + (pt.z - p1.z) * dz) /
              (dx * dx + dy * dy + dz * dz);



            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                intrPt = new Vector3d(p1.x, p1.y, p1.z);
                crvIntr = 0;
            }
            else if (t > 1)
            {
                intrPt = new Vector3d(p2.x, p2.y, p2.z);
                crvIntr = 1;
            }
            else
            {
                intrPt = new Vector3d(p1.x + t * dx, p1.y + t * dy, p1.z + t * dz);
                crvIntr = t;
            }


            //distanceSquared2D = new Vector2d(p1.x, p1.y).Distance(new Vector2d(p2.x, p2.y));
            distanceSquared3D = intrPt.Distance(pt);
            return intrPt;
        }

        public static bool SetVertex(this PlanktonVertexList vertices, int v, Vector3d pt)
        {
            return vertices.SetVertex(v, pt.x, pt.y, pt.z);
        }
    }
}
