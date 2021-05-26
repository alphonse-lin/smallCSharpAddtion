using System.Collections.Generic;
using NTS = NetTopologySuite;
using UrbanX.Application.Geometry;
using System.Collections;

namespace UrbanX.Application.Intersection
{
    public class Road
    {
        public int index { get; }
        public NTS.Geometries.LineString roadGeo { get; }
        public List<NTS.Geometries.Point> roadPts { get; }
        public int ptCount { get; set; }
        private double _dis { get; set; }

        public Road(string jsonPath)
        {

        }

        public Road(int Index, NTS.Geometries.LineString RoadGeo, double Dis)
        {
            index = Index;
            _dis = Dis;
            roadGeo = RoadGeo;
            roadPts = DividedRoad();
        }

        public List<NTS.Geometries.Point> DividedRoad()
        {
            var ptList = Poly2DCreation.DivideLineString(roadGeo, _dis, out int pointCount);
            ptCount = pointCount;
            return ptList;
        }
    }

    public class Roads
    {
        public NTS.Geometries.MultiLineString RoadGeosAsMultiLS;
        public NTS.Geometries.LineString[] RoadGeosAsLS;
        public double[] Score;

        public Roads(NTS.Geometries.LineString[] roads,double[] score)
        {
            RoadGeosAsMultiLS = new NTS.Geometries.MultiLineString(roads);
            RoadGeosAsLS = roads;
            Score = score;
        }

        public static NTS.Geometries.LineString[] ReadJson(string roadPath)
        {
            return Poly2DCreation.ReadJsonData2D<NTS.Geometries.LineString>(roadPath,JSONGeo.LineString);
        }
    }
}
