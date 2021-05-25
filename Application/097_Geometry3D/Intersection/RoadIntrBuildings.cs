using g3;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanX.Application.Geometry;
using NTS = NetTopologySuite;

namespace UrbanX.Application.Intersection
{
    public class RoadIntrBuildings
    {
        //Property
        public DMesh3 IntrMesh { get; set; }
        private List<double> ViewData { get; set; }
        public Roads RoadsValue { get; set; }

        //Filed
        private double _viewRadius;
        private double _cutRoadDis;
        private List<NTS.Geometries.LineString> _roads;
        private List<NTS.Geometries.Point> _ptLargeList;
        private DMesh3 _buildings;
        private List<int> _ptCountLargeList;
        private Dictionary<NTS.Geometries.Point, double> _ptAreaDic;
        private Dictionary<int, int> _meshCountDic;
        private VisDataType _visType;

        //Constructor
        public RoadIntrBuildings(List<NTS.Geometries.LineString> Roads, DMesh3 Buildings, Dictionary<NTS.Geometries.Point, double> PointAreaDic, double CutRoadDis, double ViewRadius, VisDataType VisType, bool exportMesh =true)
        {
            _cutRoadDis = CutRoadDis;
            _viewRadius = ViewRadius;
            _visType = VisType;

            //道路网络
            _roads = Roads;
            //道路网节点
            _ptLargeList=DivideRoads();
            //建筑物
            _buildings = Buildings;
            //初始化数据
            _ptAreaDic = PointAreaDic;
        }

        //Method
        public void Build()
        {
            //初始化颜色
            InitiateColor();

            //开始相切
            ViewData = CalcRaysThroughTri();

            //上颜色
            IntrMesh = ApplyColor();

            //赋值
            RoadsValue = VisDataInRoad();
        }

        private List<NTS.Geometries.Point> DivideRoads()
        {
            List<NTS.Geometries.Point> ptLargeList = new List<NTS.Geometries.Point>();
            List<int> ptCountList = new List<int>(_roads.Count);

            for (int i = 0; i < _roads.Count; i++)
            {
                var line = _roads[i];
                Road road = new Road(i, line, _cutRoadDis);
                ptCountList.Add(road.ptCount);
                ptLargeList.AddRange(road.roadPts);
            }

            _ptCountLargeList = ptCountList;
            return ptLargeList;
        }

        private void InitiateColor()
        {
            //初始化颜色
            MeshCreation.InitiateColor(_buildings);
        }

        private List<double> CalcRaysThroughTri() 
        {
            var visData=MeshCreation.CalcRaysThroughTri(_buildings, _ptLargeList.ToArray(), _viewRadius, _ptAreaDic, _visType, out Dictionary<int, int> meshIntrDic);
            _meshCountDic = meshIntrDic;
            return visData;
        }
        private DMesh3 ApplyColor()
        {
            return MeshCreation.ApplyColorsBasedOnRays(_buildings, _meshCountDic, Colorf.White, Colorf.Red);
        }
        private Roads VisDataInRoad()
        {
            var scoreList = new List<double>(_roads.Count);
            for (int i = 0; i < _roads.Count-1; i++)
            {
                var data = _ptCountLargeList[i];
                var next = _ptCountLargeList[i + 1];
                double temp = 0d;
                for (int j = data; j < next; j++)
                    temp += ViewData[j];

                scoreList.Add(temp);
            }

           return new Roads(_roads.ToArray(),scoreList.ToArray());
        }
            
    }

    
}
