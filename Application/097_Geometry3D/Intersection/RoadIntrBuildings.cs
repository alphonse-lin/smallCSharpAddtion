using g3;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UrbanX.Application.Geometry;
using NTS = NetTopologySuite;
using Urbanx.Application.Geometry.Extension;

namespace UrbanX.Application.Intersection
{
    public class RoadIntrBuildings
    {
        //Property
        public DMesh3 IntrMesh { get; set; }
        public List<double> ViewData { get; set; }
        public Roads RoadsValue { get; set; }

        //Filed
        private double _viewRadius;
        private double _cutRoadDis;
        private List<NTS.Geometries.LineString> _roads;
        private List<NTS.Geometries.Point> _ptLargeList;
        private DMesh3 _buildings;
        private List<int> _ptCountLargeList;
        private Dictionary<NTS.Geometries.Point, double> _ptAreaDic;
        private ConcurrentDictionary<int, int> _meshCountDic;
        private VisDataType _visType;

        private string _roadFilePath;
        private string _buildingFilePath;

        //Constructor
        public RoadIntrBuildings(List<NTS.Geometries.LineString> Roads, DMesh3 Buildings, Dictionary<NTS.Geometries.Point, double> PointAreaDic, double CutRoadDis, double ViewRadius, VisDataType VisType, DateTime start, bool exportMesh =true)
        {
            _cutRoadDis = CutRoadDis;
            _viewRadius = ViewRadius;
            _visType = VisType;

            //道路网络
            _roads = Roads;
            //道路网节点
            _ptLargeList=DivideRoads(_roads,CutRoadDis,out _ptCountLargeList);
            //建筑物
            _buildings = Buildings;
            //初始化数据
            _ptAreaDic = PointAreaDic;

            // 初始化颜色
            MeshCreation.InitiateColor(_buildings);

            //开始相切
            ViewData = MeshCreation.CalcRaysThroughTriParallel(_buildings, _ptLargeList.ToArray(),_viewRadius,_ptAreaDic,_visType, start, out ConcurrentDictionary<int, int> _meshIntrDic) ;
            _meshCountDic = _meshIntrDic;

            //上颜色
            MeshCreation.ApplyColorsBasedOnRays(_buildings, _meshCountDic, Colorf.White, Colorf.Red);
            IntrMesh = _buildings;

            //赋值
            RoadsValue = VisDataInRoad(_roads,_ptCountLargeList,ViewData);
        }

        //public method
        /// <summary>
        /// export remshed mesh as obj format, export ptArea as geojson format.
        /// </summary>
        /// <param name="BuildingFilePath"></param>
        /// <param name="Attributes">"baseHeight", "brepHeight",</param>
        /// <param name="ExportMeshPath"></param>
        /// <param name="ExportPtAreaGeoJsonPath"></param>
        public static void Prepare(string BuildingFilePath, string[] Attributes, string ExportMeshPath,string ExportPtAreaGeoJsonPath, int subdivision=3)
        {
            var inputDataCollection = MeshCreation.ReadJsonData(BuildingFilePath, Attributes[0], Attributes[1], out double[] heightCollection, out double[] baseHeightCollection);
            var simpleMeshes = MeshCreation.ExtrudeMeshListFromPtMinusTopBtn(inputDataCollection, heightCollection, baseHeightCollection, out Dictionary<NetTopologySuite.Geometries.Point, double> secPtDic, out DCurve3[][] edges);
            var fc = MeshCreation.BuildFeatureCollection(secPtDic.Keys.ToArray(), secPtDic.Values.ToArray());

            DMesh3 meshCollection = new DMesh3();
            for (int i = 0; i < simpleMeshes.Length; i++)
            {
                var remeshedMesh = MeshCreation.ReMeshHardEdge(simpleMeshes[i], edges[i], subdivision);
                var exportedMesh = remeshedMesh.pMesh2g3Mesh();
                MeshEditor.Append(meshCollection, exportedMesh);
            }

            MeshCreation.ExportMeshAsObj(ExportMeshPath, meshCollection, false);
            MeshCreation.ExportGeoJSON(fc, ExportPtAreaGeoJsonPath);
        }

        /// <summary>
        /// Calc Visdata
        /// </summary>
        /// <param name="RoadFilePath"></param>
        /// <param name="MeshLoadedFilePath"></param>
        /// <param name="PtAreaLoadedPath"></param>
        /// <param name="CutRoadDis"></param>
        /// <param name="ViewRadius"></param>
        /// <param name="VisType"></param>
        /// <param name="coloredMesh"></param>
        /// <returns></returns>
        public static Roads Build(string RoadFilePath, string MeshLoadedFilePath, string PtAreaLoadedPath, double CutRoadDis, double ViewRadius, VisDataType VisType, DateTime start, out DMesh3 IntrMesh)
        {
            var roads=Roads.ReadJson(RoadFilePath).ToList();
            var buildings= MeshCreation.ImportMesh(MeshLoadedFilePath);
            MeshCreation.ReadJsonData(PtAreaLoadedPath, "Area", out Dictionary<NTS.Geometries.Point, double> ptAreaDic);

            var Result = CalcTriWithColor(roads, buildings, CutRoadDis, ViewRadius, ptAreaDic, VisType, start, out DMesh3 outMesh);
            IntrMesh = outMesh;

            return Result;
        }

        public static Roads Build(string RoadFilePath, string MeshLoadedFilePath, string PtAreaLoadedPath, double CutRoadDis, double ViewRadius, VisDataType VisType, DateTime start, bool coloredMesh = true)
        {
            var roads = Roads.ReadJson(RoadFilePath).ToList();
            var buildings = MeshCreation.ImportMesh(MeshLoadedFilePath);
            MeshCreation.ReadJsonData(PtAreaLoadedPath, "Area", out Dictionary<NTS.Geometries.Point, double> ptAreaDic);

            return CalcTriWithoutColor(roads, buildings, CutRoadDis, ViewRadius, ptAreaDic, VisType, start);
        }

        //private method
        private static Roads CalcTriWithColor(List<NTS.Geometries.LineString> roads, DMesh3 meshIn, double CutRoadDis, double viewRadius, Dictionary<NTS.Geometries.Point, double> ptAreaDic, VisDataType visType, DateTime start, out DMesh3 MeshResult)
        {
            //得到道路观测点
            var ptLargeList = DivideRoads(roads, CutRoadDis, out List<int> ptCountLargeList);
            var fcExport = Poly2DCreation.BuildFeatureCollection(ptLargeList.ToArray());
            var debugPath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\road\debug_path.geojson";
            MeshCreation.ExportGeoJSON(fcExport, debugPath);

            ToolManagers.TimeCalculation(start, "打断点");
            
            //初始化颜色
            MeshCreation.InitiateColor(meshIn);
            ToolManagers.TimeCalculation(start, "初始化颜色");

            //开始相切
            var visData = MeshCreation.CalcRaysThroughTriParallel(meshIn, ptLargeList.ToArray(), viewRadius, ptAreaDic, visType, start, out ConcurrentDictionary<int, int> meshIntrDic);
            ToolManagers.TimeCalculation(start, "开始相切");

            //上颜色
            var IntrMesh=MeshCreation.ApplyColorsBasedOnRays(meshIn, meshIntrDic, Colorf.White, Colorf.Red);
            MeshResult = IntrMesh;
            ToolManagers.TimeCalculation(start, "上颜色");


            //赋值
            return VisDataInRoad(roads,ptCountLargeList,visData);
        }
        private static Roads CalcTriWithoutColor(List<NTS.Geometries.LineString> roads, DMesh3 meshIn, double CutRoadDis, double viewRadius, Dictionary<NTS.Geometries.Point, double> ptAreaDic, VisDataType visType, DateTime start)
        {
            //得到道路观测点
            var ptLargeList = DivideRoads(roads, CutRoadDis, out List<int> ptCountLargeList);

            //开始相切
            var visData = MeshCreation.CalcRaysThroughTriParallel(meshIn, ptLargeList.ToArray(), viewRadius, ptAreaDic, visType, start, out ConcurrentDictionary<int, int> meshIntrDic);

            //赋值
            return VisDataInRoad(roads, ptCountLargeList, visData);
        }
        private static List<NTS.Geometries.Point> DivideRoads(List<NTS.Geometries.LineString> _roads, double _cutRoadDis, out List<int> _ptCountLargeList)
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
        private static Roads VisDataInRoad(List<NTS.Geometries.LineString> _roads, List<int> _ptCountLargeList, List<double> ViewData)
        {
            var scoreList = new List<double>(_roads.Count);
            var resultList = new List<double>(_roads.Count);
            List<int> re_ptCountLargeList = new List<int>() { 0 };
            re_ptCountLargeList.AddRange(_ptCountLargeList);
            for (int i = 0; i < re_ptCountLargeList.Count-1; i++)
            {
                var before = re_ptCountLargeList[i];
                var data = re_ptCountLargeList[i+1];
                    
                double temp = 0d;
                for (int j = before; j < before+data; j++)
                    temp += ViewData[j];

                scoreList.Add(temp);
            }

            for (int i = 0; i < scoreList.Count; i++)
            {
                var score = scoreList[i];
                if (score != 0)
                    resultList.Add(score / _ptCountLargeList[i]);
                else
                    resultList.Add(0);
                   
            }

           return new Roads(_roads.ToArray(), resultList.ToArray());
        }
            
    }

    
}
