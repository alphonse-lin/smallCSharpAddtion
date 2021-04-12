using System;
using System.Collections.Generic;
using System.Text;
using NTSGeometry=NetTopologySuite.Geometries;
using g3;
using System.IO;
using NTS=NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Predicate;
using NetTopologySuite.Index;
using System.Linq;

namespace UrbanX_GH.Application.Geometry
{
    public class Poly2DCreation
    {
        public static NTSGeometry.Geometry[] CreateCircle(Vector2d[] origins, double radius)
        {
            var count = origins.Length;
            var circleList = new NTSGeometry.Geometry[count];
            for (int i = 0; i < count; i++)
            {
                NTSGeometry.Point temp = new NTSGeometry.Point(origins[i].x, origins[i].y);
                var circle = temp.Buffer(radius,NetTopologySuite.Operation.Buffer.EndCapStyle.Round);
                circleList[i] = circle;
            }
            return circleList;
        }

        public static NTSGeometry.Geometry[] CreateCircle(Vector2d[] origins, double[] radius)
        {
            var count = origins.Length;
            var circleList = new NTSGeometry.Geometry[count];
            for (int i = 0; i < count; i++)
            {
                NTSGeometry.Point temp = new NTSGeometry.Point(origins[i].x, origins[i].y);
                var circle = temp.Buffer(radius[i], NetTopologySuite.Operation.Buffer.EndCapStyle.Round);
                circleList[i] = circle;
            }
            return circleList;
        }

        public static Polygon[] CreatePolygon(Coordinate[][] polygonVertices)
        {
            var count = polygonVertices.Length;
            var polygonList = new Polygon[count];
            for (int i = 0; i < count; i++)
            {
                Polygon polygon = new Polygon(new LinearRing(polygonVertices[i]));
                polygonList[i] = polygon;
            }
            return polygonList;

        }

        /// <summary>
        /// 计算被包含的点
        /// </summary>
        /// <param name="mainPtList"></param>
        /// <param name="secPtList"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static List<List<NTSGeometry.Point>> ContainsInPts(NTSGeometry.Point[] mainPtList, NTSGeometry.Point[] secPtList, double[] radius)
        {
            NTS.Index.Quadtree.Quadtree<NTSGeometry.Point> quadTree = new NTS.Index.Quadtree.Quadtree<NTSGeometry.Point>();
            for (int i = 0; i < secPtList.Length; i++)
                quadTree.Insert(secPtList[i].EnvelopeInternal, secPtList[i]);

            List<List<NTSGeometry.Point>> secPtListCollection = new List<List<NTSGeometry.Point>>(mainPtList.Length);
            for (int i = 0; i < mainPtList.Length; i++)
            {
                var mainCoor = new Coordinate(mainPtList[i].X, mainPtList[i].Y);
                var tempEnv=CreateEnvelopeFromPt(mainPtList[i], radius[i]);
                var secPtListQuery= quadTree.Query(tempEnv);
                
                List<NTSGeometry.Point> secPtContain = new List<NTSGeometry.Point>();
                for (int j = 0; j < secPtListQuery.Count; j++)
                {
                    var secPt = secPtListQuery[j];
                    
                    Coordinate secCoor = new Coordinate(secPt.X,secPt.Y);
                    double dis=mainCoor.Distance(secCoor);
                    if (dis < radius[i])
                        secPtContain.Add(secPt);
                }
                secPtListCollection.Add(secPtContain);
            }
            return secPtListCollection;
        }

        public static List<List<NTSGeometry.Point>> ContainsInPts(NTSGeometry.Point[] mainPtList, NTSGeometry.Point[] secPtList, double radius=300)
        {
            NTS.Index.Quadtree.Quadtree<NTSGeometry.Point> quadTree = new NTS.Index.Quadtree.Quadtree<NTSGeometry.Point>();
            for (int i = 0; i < secPtList.Length; i++)
                quadTree.Insert(secPtList[i].EnvelopeInternal, secPtList[i]);

            List<List<NTSGeometry.Point>> secPtListCollection = new List<List<NTSGeometry.Point>>(mainPtList.Length);
            for (int i = 0; i < mainPtList.Length; i++)
            {
                var mainCoor = new Coordinate(mainPtList[i].X, mainPtList[i].Y);
                var tempEnv = CreateEnvelopeFromPt(mainPtList[i], radius);
                var secPtListQuery = quadTree.Query(tempEnv);

                List<NTSGeometry.Point> secPtContain = new List<NTSGeometry.Point>();
                for (int j = 0; j < secPtListQuery.Count; j++)
                {
                    var secPt = secPtListQuery[j];

                    Coordinate secCoor = new Coordinate(secPt.X, secPt.Y);
                    double dis = mainCoor.Distance(secCoor);
                    if (dis < radius)
                        secPtContain.Add(secPt);
                }
                secPtListCollection.Add(secPtContain);
            }
            return secPtListCollection;
        }

        //TODO 划分点，输出mainPtList
        //public static NTSGeometry.Point[] DividePolyline()
        //{
        //    var dividedPtList=
        //}

        /// <summary>
        /// 计算点内所包含的所有面积
        /// </summary>
        /// <param name="containsInPtsData"></param>
        /// <param name="areaDic"> point is 2d point</param>
        /// <returns></returns>
        public static double[] ContainsAreaInPts(List<List<NTSGeometry.Point>> containsInPtsData, Dictionary<NTSGeometry.Point, double> areaDic)
        {
            double[] areaResult = new double[containsInPtsData.Count];
            for (int i = 0; i < containsInPtsData.Count; i++)
            {
                var ptListInMainPt = containsInPtsData[i];
                List<double> areaList = new List<double>(ptListInMainPt.Count);
                for (int j = 0; j < ptListInMainPt.Count; j++)
                {
                    var single = areaDic[ptListInMainPt[j]];
                    areaList.Add(single);
                }
                areaResult[i]= areaList.Sum();
            }
            return areaResult;
        }

        private static Envelope CreateEnvelopeFromPt(NTSGeometry.Point origin, double radius)
        {
            var ptLeftDown = new NTSGeometry.Coordinate(origin.X - radius, origin.Y - radius);
            var ptRightUp = new NTSGeometry.Coordinate(origin.X + radius, origin.Y + radius);
            return new Envelope(ptLeftDown, ptRightUp);
        }
    }
}
