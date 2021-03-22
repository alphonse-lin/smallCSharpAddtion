using System;
using System.Collections.Generic;
using System.Linq;

using Rhino.Geometry;


namespace UrbanX.Planning.UrbanDesign
{
    public class SiteParameters : IDisposable
    {
        public Curve Site { get; private set; }

        public double Radiant { get; private set; }

        // Need to use the largest score to get following parameters.
        public double[] Scores { get; private set; }

        public double Density { get; private set; }

        public double FAR { get; private set; }

        public int SiteType { get; private set; }

        public double MixRatio { get; private set; }

        public int BuildingStyle { get; private set; }


        /// <summary>
        /// Constructor for Refine site parameters in the SiteMainMethods class.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="radiant"></param>
        /// <param name="scores"></param>
        /// <param name="density"></param>
        /// <param name="far"></param>
        /// <param name="siteType"></param>
        /// <param name="mixRatio"></param>
        public SiteParameters(Curve site, double radiant, double[] scores, double density, double far, int siteType, double mixRatio, int buildingStyle)
        {
            Site = site.DuplicateCurve();
            // Site curve should be in the clockwise order.
            if (site.ClosedCurveOrientation() == CurveOrientation.CounterClockwise)
                site.Reverse();

            Radiant = radiant;
            Scores = scores;
            Density = density;
            FAR = far;
            SiteType = siteType;
            MixRatio = mixRatio;
            BuildingStyle = buildingStyle;
        }


        /// <summary>
        /// Basic constructor for getting the site parameters from intial roads and accessibilty.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="allRoadsMidPts"></param>
        /// <param name="allScores"></param>
        public SiteParameters(Curve site, Point3d[] allRoadsMidPts, double[] allScores, double tolerance)
        {
            if (allRoadsMidPts.Length != allScores.Length)
                throw new ArgumentOutOfRangeException("All roads segments should have and only have one score.");

            // Site curve should be in the clockwise order.
            if (site.ClosedCurveOrientation() == CurveOrientation.CounterClockwise)
                site.Reverse();

            Site = site;
            Radiant = GetRadiant(tolerance);

            Scores = GetScores(allRoadsMidPts, allScores);

            //Random parameters
            SiteType = GetRandomType();
            Density = GetRandomDensity();
            FAR = GetRandomFar();
            MixRatio = GetRandomMix();

            // Default parameters
            BuildingStyle = 0;
        }



        public void SetDensity(double value) => Density = value;

        public void SetFar(double value) => FAR = value;

        public void SetSiteType(int value) => SiteType = value;

        public void SetMixRatio(double value) => MixRatio = value;

        public void SetBuildingStyle(int value) => BuildingStyle = value;



        private int GetRandomType()
        {
            Random r = new Random();

            // U is the sixest type.
            return r.Next(5);
        }

        private double GetRandomDensity()
        {
            Random r = new Random();
            var t = (SiteTypes)SiteType;
            var interval = SiteDataset.GetDensityInterval(t);

            return r.NextDouble() * (interval[1] - interval[0]) + interval[0];
        }

        private double GetRandomFar()
        {
            Random r = new Random();
            var t = (SiteTypes)SiteType;
            var interval = SiteDataset.GetFarInterval(t);

            return r.NextDouble() * (interval[1] - interval[0]) + interval[0];
        }

        private double GetRandomMix()
        {
            Random r = new Random();
            return r.NextDouble() * 0.3;
        }




        public double GetRadiant(double tolerance)
        {
            // Get control polygon count.
            var polyline = DesignToolbox.ConvertToPolyline(Site, tolerance);
            polyline.ReduceSegments(1);

            // Reset site curve.
            Site = polyline.ToPolylineCurve();
            var segs = polyline.GetSegments();

            List<double> lens = new List<double>(segs.Length);
            for (int i = 0; i < segs.Length; i++)
            {
                lens.Add(segs[i].Length);
            }

            var max = segs[lens.IndexOf(lens.Max())];

            Vector3d v = new Vector3d(max.To - max.From);
            v.Unitize();
            if (v.Y < 0)
                v.Reverse();

            double radiant= Math.Acos(Math.Max(Math.Min(v * Vector3d.XAxis, 1.0), -1.0));

            radiant = radiant > Math.PI * 0.5 ? Math.PI - radiant : radiant;
            radiant = radiant > Math.PI * 0.25 ? Math.PI * 0.5 - radiant : radiant;

            return radiant;
        }

        private double[] GetScores(Point3d[] allRoadsMidPts, double[] allScores)
        {
            var needles = GetEdgesMidPoints(Site, Radiant);
            var indices = FindCloestPoints(allRoadsMidPts, needles);

            // needles.length should be 4.
            double[] result = new double[needles.Length];
            for (int i = 0; i < 4; i++)
            {
                result[i] = allScores[indices[i]];
            }

            return result;
        }


        private Point3d[] GetEdgesMidPoints(Curve curve, double radiant)
        {
            Plane worldxy = Plane.WorldXY;
            Plane origin = worldxy.Clone();
            worldxy.Rotate(radiant, worldxy.ZAxis);
            var transform = Transform.ChangeBasis(origin, worldxy);

            // BoundingBox is in orientation plane coordinate. Need to locate in world coordinate.
            var box = curve.GetBoundingBox(transform);

            // Should be planary rectangle. Locate box in world coordinate.
            Box locateBox = new Box(worldxy, new Interval(box.Min.X, box.Max.X), new Interval(box.Min.Y, box.Max.Y), new Interval(0, 0));

            var pts = locateBox.GetCorners().ToList().GetRange(0, 4);

            var result = new Point3d[4];
            for (int i = 0; i < 4; i++)
            {
                if (i == 3)
                {
                    result[i] = (pts[i] + pts[0]) / 2;
                    break;
                }
                result[i] = (pts[i] + pts[i + 1]) / 2;
            }
            return result;
        }


        private int[] FindCloestPoints(IList<Point3d> allNodes, IList<Point3d> needles)
        {
            var indicesArray = RTree.Point3dClosestPoints(allNodes, needles, double.MaxValue).ToArray();

            int[] result = new int[indicesArray.Length];
            for (int i = 0; i < indicesArray.Length; i++)
            {
                result[i] = indicesArray[i][0];
            }

            return result;
        }

        public void Dispose()
        {
            Site.Dispose();
            Scores = null;
        }
    }
}
