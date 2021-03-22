using System;
using System.Collections.Generic;
using Rhino.Geometry;

using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using System.Linq;

using UrbanX.DataStructures.Geometry;

namespace UrbanX.Planning.ToJSON
{
    public enum GeometryType
    {
        Road,
        Block,
        Subsite,
        Building,
        Brep,
        SubsiteSetback,
        BuildingLayers,
        BuildingOutLines,
        RoadPoints,
        RoadPoints_Rays,
        Debug
    }
    public class ConvertToJson
    {
        private static double[,] CreatePt(Polyline polyline, out double baseHeight)
        {
            int roundNum = 6;
            double[,] ptList = new double[polyline.Count,2];
            var pz = polyline.Z[0];
            for (int i = 0; i < polyline.Count; i++)
            {
                var px = polyline.X[i];
                var py = polyline.Y[i];

                ptList[i,0] = Math.Round(px, roundNum);
                ptList[i,1] = Math.Round(py, roundNum);
            }
            baseHeight = pz;
            return ptList;
        }

        private static double[,] CreatePt(Polyline polyline, double xCorrect, double yCorrect,out double baseHeight)
        {
            int roundNum = 6;
            double[,] ptList = new double[polyline.Count, 2];
            var pz = polyline.Z[0];
            for (int i = 0; i < polyline.Count; i++)
            {
                var px = polyline.X[i]+xCorrect;
                var py = polyline.Y[i]+yCorrect;

                ptList[i, 0] = Math.Round(px, roundNum);
                ptList[i, 1] = Math.Round(py, roundNum);
            }
            baseHeight = pz;
            return ptList;
        }
        private static double[,] CreatePt(Polyline polyline, double xCorrect, double yCorrect)
        {
            int roundNum = 6;
            double[,] ptList = new double[polyline.Count, 2];
            var pz = polyline.Z[0];
            for (int i = 0; i < polyline.Count; i++)
            {
                var px = polyline.X[i]+ xCorrect;
                var py = polyline.Y[i]+ yCorrect;

                ptList[i, 0] = Math.Round(px, roundNum);
                ptList[i, 1] = Math.Round(py, roundNum);
            }
            return ptList;
        }

        private static Polyline ChangeIntoPolyLine(Curve crv)
        {
            bool flag=crv.TryGetPolyline(out Polyline polyline);
            if (!flag)
            {
                var length = crv.GetLength();
                var polylinCrv = crv.ToPolyline(0.0001, 0.0001, length / 200, length / 2);
                return polylinCrv.ToPolyline();
            }
            return polyline;
        }
  
        /// <summary>
        /// create json file based on road
        /// </summary>
        /// <param name="crvList"></param>
        /// <param name="ssValue"></param>
        /// <returns></returns>
        public static string CreateJson_Road(List<Curve> crvList, IEnumerable<double> ssValue)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < crvList.Count; i++)
            {
                var polyline = ChangeIntoPolyLine(crvList[i]);
                var ptArray = CreatePt(polyline, out _);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length/2; j++)
                {
                    JArray ptValue = new JArray(ptArray[j, 0], ptArray[j, 1]);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                JProperty j_ssValue = new JProperty("ssValue",ssValue.ElementAt(i));
                JProperty jprots = new JProperty("properties", new JObject(j_ssValue));

                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                jfeslst.Add(jtfea);
            }
            JProperty jname = new JProperty("name", "roadGeoJSON");

            JProperty jcrProProjection=new JProperty("name","urn:ogc:def:crs:EPSG::32650");
            JObject jcrs_Pro =new JObject(new JProperty("type","name"),jcrProProjection);

            JProperty jcrs = new JProperty("crs", jcrs_Pro);
            JProperty jfs = new JProperty("features", jfeslst);
            
            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname,jcrs,jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }
        public static string CreateJson_Road(List<Curve> crvList, IEnumerable<double> ssValue, double coordX, double coordY)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < crvList.Count; i++)
            {
                var polyline = ChangeIntoPolyLine(crvList[i]);
                var ptArray = CreatePt(polyline, coordX, coordY);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length / 2; j++)
                {
                    JArray ptValue = new JArray(ptArray[j, 0], ptArray[j, 1]);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                JProperty j_ssValue = new JProperty("ssValue", ssValue.ElementAt(i));
                JProperty jprots = new JProperty("properties", new JObject(j_ssValue));

                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                jfeslst.Add(jtfea);
            }
            JProperty jname = new JProperty("name", "roadGeoJSON");

            //Coordinates
            JProperty jcoordinatesValueX = new JProperty("xVlaue", coordX);
            JProperty jcoordinatesValueY = new JProperty("yValue", coordY);
            JProperty jcoordinates = new JProperty("properties", new JObject(jcoordinatesValueX, jcoordinatesValueY));
            JObject jcoordinates_Pro = new JObject(new JProperty("type", "name"), jcoordinates);
            JProperty jcoordinatesFinal = new JProperty("correctCoordinates", jcoordinates_Pro);

            //crs
            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JProperty jcrs = new JProperty("properties", new JObject(jcrProProjection));
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrs);
            JProperty jcrsFinal = new JProperty("crs", jcrs_Pro);

            //核心内容
            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcoordinatesFinal, jcrsFinal, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }
        public static string CreateJson_Block(List<Curve> crvList, List<int> BlockID, List<double> bdTotalArea, List<double> FAR, List<double> Density, List<int[]> EConsumption, List<int[]> WConsumption, List<int[]> GConsumption)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < crvList.Count; i++)
            {
                var polyline = ChangeIntoPolyLine(crvList[i]);
                var ptArray = CreatePt(polyline,out _);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length / 2; j++)
                {
                    JArray ptValue = new JArray(ptArray[j, 0], ptArray[j, 1]);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                JProperty j_BlockID = new JProperty("blockID", BlockID[i]);
                JProperty j_bdTotalArea = new JProperty("totalArea", bdTotalArea[i]);
                JProperty j_FAR = new JProperty("FAR", FAR[i]);
                JProperty j_Density = new JProperty("Density", Density[i]);

                JArray econValue = new JArray(EConsumption[i][0], EConsumption[i][1]);
                JProperty j_EConsumption = new JProperty("EC", econValue);

                JArray wconValue = new JArray(WConsumption[i][0], WConsumption[i][1]);
                JProperty j_WConsumption = new JProperty("WC", wconValue);

                JArray gconValue = new JArray(GConsumption[i][0], GConsumption[i][1]);
                JProperty j_GConsumption = new JProperty("GC", gconValue);
                
                JProperty jprots = new JProperty("properties", new JObject(j_BlockID,j_bdTotalArea, j_FAR,j_Density,j_EConsumption,j_WConsumption,j_GConsumption));

                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                jfeslst.Add(jtfea);
            }

            JProperty jname = new JProperty("name", "blockGeoJSON");

            //crs
            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JProperty jcrs = new JProperty("properties", new JObject(jcrProProjection));
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrs);
            JProperty jcrsFinal = new JProperty("crs", jcrs_Pro);

            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcrsFinal, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }
        public static string CreateJson_Block(List<Curve> crvList, List<int> BlockID, List<double> bdTotalArea, List<double> SSValue, List<double> FAR, List<double> Density, List<int[]> EConsumption, List<int[]> WConsumption,  List<int[]> GConsumption, double coordX, double coordY)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < crvList.Count; i++)
            {
                var polyline = ChangeIntoPolyLine(crvList[i]);
                var ptArray = CreatePt(polyline, coordX, coordY);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length / 2; j++)
                {
                    JArray ptValue = new JArray(ptArray[j, 0], ptArray[j, 1]);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                JProperty j_BlockID = new JProperty("blockID", BlockID[i]);
                JProperty j_bdTotalArea = new JProperty("totalArea", bdTotalArea[i]);
                JProperty j_SSValue = new JProperty("ssValue", SSValue[i]);
                JProperty j_FAR = new JProperty("FAR", FAR[i]);
                JProperty j_Density = new JProperty("Density", Density[i]);

                JArray econValue = new JArray(EConsumption[i][0], EConsumption[i][1],EConsumption[i][2]);
                JProperty j_EConsumption = new JProperty("EC", econValue);
                //JArray econMidValue = new JArray(EConsumptionMid[i]);
                //JProperty j_EConsumptionMid = new JProperty("ECMid", econMidValue);

                JArray wconValue = new JArray(WConsumption[i][0], WConsumption[i][1], WConsumption[i][2]);
                JProperty j_WConsumption = new JProperty("WC", wconValue);
                //JArray wconMidValue = new JArray(WConsumptionMid[i]);
                //JProperty j_WConsumptionMid = new JProperty("WCMid", wconMidValue);

                JArray gconValue = new JArray(GConsumption[i][0], GConsumption[i][1], GConsumption[i][2]);
                JProperty j_GConsumption = new JProperty("GC", gconValue);
                //JArray gconMidValue = new JArray(GConsumptionMid[i]);
                //JProperty j_GConsumptionMid = new JProperty("GCMid", gconMidValue);

                JProperty jprots = new JProperty("properties", new JObject(j_BlockID, j_bdTotalArea, j_SSValue,j_FAR, j_Density, j_EConsumption, j_WConsumption, j_GConsumption));

                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                jfeslst.Add(jtfea);
            }

            JProperty jname = new JProperty("name", "blockGeoJSON");

            //crs
            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JProperty jcrs = new JProperty("properties", new JObject(jcrProProjection));
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrs);
            JProperty jcrsFinal = new JProperty("crs", jcrs_Pro);

            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcrsFinal, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }

        public static string CreateJson_Subsite(List<Curve> crvList, List<int> SubsiteBlockID, List<int> BlockID)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < crvList.Count; i++)
            {
                var polyline = ChangeIntoPolyLine(crvList[i]);
                var ptArray = CreatePt(polyline, out _);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length / 2; j++)
                {
                    JArray ptValue = new JArray(ptArray[j, 0], ptArray[j, 1]);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                JProperty j_subsetBlockID = new JProperty("subsiteID", SubsiteBlockID[i]);
                JProperty j_BlockID = new JProperty("blockID", BlockID[i]);

                JProperty jprots = new JProperty("properties", new JObject(j_subsetBlockID, j_BlockID));

                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                jfeslst.Add(jtfea);
            }

            JProperty jname = new JProperty("name", "subsiteGeoJSON");

            //crs
            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JProperty jcrs = new JProperty("properties", new JObject(jcrProProjection));
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrs);
            JProperty jcrsFinal = new JProperty("crs", jcrs_Pro);

            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcrsFinal, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }

        public static string CreateJson_Subsite(List<Curve> crvList, List<int> SubsiteBlockID, List<int> BlockID, double coordX, double coordY)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < crvList.Count; i++)
            {
                var polyline = ChangeIntoPolyLine(crvList[i]);
                var ptArray = CreatePt(polyline, coordX, coordY);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length / 2; j++)
                {
                    JArray ptValue = new JArray(ptArray[j, 0], ptArray[j, 1]);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                JProperty j_subsetBlockID = new JProperty("subsiteID", SubsiteBlockID[i]);
                JProperty j_BlockID = new JProperty("blockID", BlockID[i]);

                JProperty jprots = new JProperty("properties", new JObject(j_subsetBlockID, j_BlockID));

                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                jfeslst.Add(jtfea);
            }

            JProperty jname = new JProperty("name", "subsiteGeoJSON");

            //crs
            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JProperty jcrs = new JProperty("properties", new JObject(jcrProProjection));
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrs);
            JProperty jcrsFinal = new JProperty("crs", jcrs_Pro);

            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcrsFinal, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }

        public static string CreateJson_BuildingBrep(List<Curve> crvList, List<int> BrepID, List<int> BuildingID, List<int> SubsetID, List<int> BlockID, List<double> bdArea, List<int> AmOfLayer, List<string>Function, List<double[]> EConsumption, List<double[]> WConsumption, List<double[]> GConsumption)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < crvList.Count; i++)
            {
                var polyline = ChangeIntoPolyLine(crvList[i]);
                var ptArray = CreatePt(polyline, out double pz);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length / 2; j++)
                {
                    JArray ptValue = new JArray(ptArray[j, 0], ptArray[j, 1]);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                JProperty j_brepID = new JProperty("brepID", BrepID[i]);
                JProperty j_bdID = new JProperty("buildingID",BuildingID[i]);
                JProperty j_subsiteID = new JProperty("subiteID", SubsetID[i]);
                JProperty j_blockID = new JProperty("blockID", BlockID[i]);
                JProperty j_bdHeight = new JProperty("baseHeight", pz);
                JProperty j_bdArea = new JProperty("totalArea", bdArea[i]);
                JProperty j_AmOfLayer = new JProperty("floors", AmOfLayer[i]);
                JProperty j_Density = new JProperty("function", Function[i]);

                JArray econValue = new JArray(EConsumption[i][0], EConsumption[i][1]);
                JProperty j_EConsumption = new JProperty("EC", econValue);

                JArray wconValue = new JArray(WConsumption[i][0], WConsumption[i][1]);
                JProperty j_WConsumption = new JProperty("WC", wconValue);

                JArray gconValue = new JArray(GConsumption[i][0], GConsumption[i][1]);
                JProperty j_GConsumption = new JProperty("GC", gconValue);

                JProperty jprots = new JProperty("properties", new JObject(j_brepID, j_bdID, j_subsiteID, j_blockID, j_bdHeight, j_bdArea, j_AmOfLayer, j_Density, j_EConsumption, j_WConsumption, j_GConsumption));

                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                jfeslst.Add(jtfea);
            }
            JProperty jname = new JProperty("name", "BuildingGeoJSON");

            //crs
            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JProperty jcrs = new JProperty("properties", new JObject(jcrProProjection));
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrs);
            JProperty jcrsFinal = new JProperty("crs", jcrs_Pro);

            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcrsFinal, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }

        public static string CreateJson_BuildingBrep(List<Curve> crvList, List<int> BrepID, List<int> BuildingID, List<int> SubsetID, List<int> BlockID, List<double> brepHeight, List<double> bdArea, List<int> AmOfLayer, List<string> Function, List<int[]> EConsumption, List<int[]> WConsumption, List<int[]> GConsumption, double coordX, double coordY)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < crvList.Count; i++)
            {
                var polyline = ChangeIntoPolyLine(crvList[i]);
                var ptArray = CreatePt(polyline, coordX, coordY, out double pz);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length / 2; j++)
                {
                    JArray ptValue = new JArray(ptArray[j, 0], ptArray[j, 1]);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                JProperty j_brepID = new JProperty("brepID", BrepID[i]);
                JProperty j_bdID = new JProperty("buildingID", BuildingID[i]);
                JProperty j_subsiteID = new JProperty("subsiteID", SubsetID[i]);
                JProperty j_blockID = new JProperty("blockID", BlockID[i]);
                JProperty j_bdHeight = new JProperty("baseHeight", pz);
                JProperty j_brepHeight = new JProperty("brepHeight", brepHeight[i]);
                JProperty j_bdArea = new JProperty("totalArea", bdArea[i]);
                JProperty j_AmOfLayer = new JProperty("floors", AmOfLayer[i]);
                JProperty j_Density = new JProperty("function", Function[i]);

                JArray econValue = new JArray(EConsumption[i][0], EConsumption[i][1], EConsumption[i][2]);
                JProperty j_EConsumption = new JProperty("EC", econValue);
                //JArray econMidValue = new JArray(ECMid[i]);
                //JProperty j_EConsumptionMid = new JProperty("ECMid", econMidValue);

                JArray wconValue = new JArray(WConsumption[i][0], WConsumption[i][1], WConsumption[i][2]);
                JProperty j_WConsumption = new JProperty("WC", wconValue);
                //JArray wconMidValue = new JArray(WCMid[i]);
                //JProperty j_WConsumptionMid = new JProperty("WCMid", wconMidValue);

                JArray gconValue = new JArray(GConsumption[i][0], GConsumption[i][1], GConsumption[i][2]);
                JProperty j_GConsumption = new JProperty("GC", gconValue);
                //JArray gconMidValue = new JArray(GCMid[i]);
                //JProperty j_GConsumptionMid = new JProperty("GCMid", gconMidValue);

                JProperty jprots = new JProperty("properties", new JObject(j_brepID, j_bdID, j_subsiteID, j_blockID, j_bdHeight, j_brepHeight,j_bdArea, j_AmOfLayer, j_Density, j_EConsumption, j_WConsumption, j_GConsumption));

                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                jfeslst.Add(jtfea);
            }
            JProperty jname = new JProperty("name", "BuildingGeoJSON");

            //crs
            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JProperty jcrs = new JProperty("properties", new JObject(jcrProProjection));
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrs);
            JProperty jcrsFinal = new JProperty("crs", jcrs_Pro);

            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcrsFinal, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }

        public static string CreateJson_Visual(List<Curve> crvList, string name)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < crvList.Count; i++)
            {
                var polyline = ChangeIntoPolyLine(crvList[i]);
                var ptArray = CreatePt(polyline, out _);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length / 2; j++)
                {
                    JArray ptValue = new JArray(ptArray[j, 0], ptArray[j, 1]);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                JProperty jprots = new JProperty("properties");

                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                jfeslst.Add(jtfea);
            }
            JProperty jname = new JProperty("name", name);

            //crs
            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JProperty jcrs = new JProperty("properties", new JObject(jcrProProjection));
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrs);
            JProperty jcrsFinal = new JProperty("crs", jcrs_Pro);

            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcrsFinal, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }

        public static string CreateJson_Visual(List<Curve> crvList, string name, double coordX, double coordY)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < crvList.Count; i++)
            {
                var polyline = ChangeIntoPolyLine(crvList[i]);
                var ptArray = CreatePt(polyline, coordX, coordY);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length / 2; j++)
                {
                    JArray ptValue = new JArray(ptArray[j, 0], ptArray[j, 1]);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                //JProperty jprots = new JProperty("properties");
                JObject jprots_Pro = new JObject();
                JProperty jprotsFinal = new JProperty("properties",jprots_Pro);

                //JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprotsFinal,jgomtry);
                jfeslst.Add(jtfea);
            }
            JProperty jname = new JProperty("name", name);

            //crs
            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JProperty jcrs = new JProperty("properties", new JObject(jcrProProjection));
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrs);
            JProperty jcrsFinal = new JProperty("crs", jcrs_Pro);

            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcrsFinal, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }

        public static string CreateJson_Visual(List<Polyline> crvList, string name, double coordX, double coordY)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < crvList.Count; i++)
            {
                var ptArray = CreatePt(crvList[i], coordX, coordY);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length / 2; j++)
                {
                    JArray ptValue = new JArray(ptArray[j, 0], ptArray[j, 1]);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                //JProperty jprots = new JProperty("properties");
                JObject jprots_Pro = new JObject();
                JProperty jprotsFinal = new JProperty("properties", jprots_Pro);

                //JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprotsFinal, jgomtry);
                jfeslst.Add(jtfea);
            }
            JProperty jname = new JProperty("name", name);

            //crs
            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JProperty jcrs = new JProperty("properties", new JObject(jcrProProjection));
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrs);
            JProperty jcrsFinal = new JProperty("crs", jcrs_Pro);

            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcrsFinal, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }

        private List<JArray> ConvertToJArrayList(double[] arrayValue)
        {
            List<JArray> ptValueList = new List<JArray>();
            for (int j = 0; j < arrayValue.Count() / 2; j++)
            {
                JArray ptValue = new JArray(arrayValue[0], arrayValue[1]);
                ptValueList.Add(ptValue);
            }
            return ptValueList;
        }
        
        public static void ExportJsonFile(string jsonString, string directory,GeometryType gType )
        {
            string path = null;
            switch (gType)
            {
                case GeometryType.Road:
                    path = Path.Combine(directory, "road.geojson");
                    break;
                case GeometryType.Block:
                    path = Path.Combine(directory, "block.geojson");
                    break;
                case GeometryType.Subsite:
                    path = Path.Combine(directory, "subsite.geojson");
                    break;
                case GeometryType.Building:
                    path = Path.Combine(directory, "building.geojson");
                    break;
                case GeometryType.Brep:
                    path = Path.Combine(directory, "brep.geojson");
                    break;
                case GeometryType.SubsiteSetback:
                    path = Path.Combine(directory, "green.geojson");
                    break;
                case GeometryType.BuildingLayers:
                    path = Path.Combine(directory, "buildingLayers.geojson");
                    break;
                case GeometryType.Debug:
                    path = Path.Combine(directory, "debugFile.geojson");
                    break;
            }

            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            sw.Write(jsonString);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }
        
        /// <summary>
        /// read geoJson File
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outPolyLineList"></param>
        public static void ReadJson(string inputFile, out List<Polyline> outPolyLineList, out List<double> outDensityList, out List<int> outSiteTypeList, out List<double> outMixRList, out List<double> outFARList, out List<int> outBStyleList)
        {
            StreamReader sr = new StreamReader(inputFile, Encoding.UTF8);
            JObject o = JObject.Parse(sr.ReadToEnd());

            JToken jfts = o["features"];

            List<JToken> jlst = jfts.ToList<JToken>();
            int jlen = jlst.Count;

            List<Polyline> polyLineList = new List<Polyline>(jlen);
            List<double> densityList = new List<double>(jlen);
            List<int> siteTypeList = new List<int>(jlen);
            List<double> mixRList = new List<double>(jlen);
            List<double> FARList = new List<double>(jlen);
            List<int> BStyleList = new List<int>(jlen);
            for (int i = 0; i < jlen; i++)
            {
                string jt1 = (string)jlst[i]["geometry"]["type"];
                if (jt1 == "LineString")
                {
                    try
                    {
                        var poiArr = jlst[i]["geometry"]["coordinates"];
                        var itemProperties = poiArr.Children<JArray>();

                        Point3d[] ptArray = new Point3d[itemProperties.Count()];
                        for (int j = 0; j < itemProperties.Count(); j++)
                        {
                            ptArray[j] = new Point3d(double.Parse(itemProperties.ElementAt(j)[0].ToString()), double.Parse(itemProperties.ElementAt(j)[1].ToString()), 0d);
                        }

                        Polyline polyLine = new Polyline(ptArray);
                        polyLineList.Add(polyLine);

                        var propertyArr = jlst[i]["geometry"]["coordinates"];
                        JToken gprop = jlst[i]["properties"];

                        ParseJsonProperty(0, gprop, densityList);
                        ParseJsonProperty(1, gprop, siteTypeList);
                        ParseJsonProperty(2, gprop, mixRList);
                        ParseJsonProperty(3, gprop, FARList);
                        ParseJsonProperty(4, gprop, BStyleList);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else { }
            }
            outPolyLineList = polyLineList;
            outDensityList = densityList;
            outSiteTypeList = siteTypeList;
            outMixRList = mixRList;
            outFARList = FARList;
            outBStyleList = BStyleList;
        }

        public static void ReadJson(string inputFile, double coordX, double coordY, out List<Polyline> outPolyLineList, out List<string> outDensityList, out List<string> outSiteTypeList, out List<string> outMixRList, out List<string> outFARList, out List<string> outBStyleList)
        {
            StreamReader sr = new StreamReader(inputFile, Encoding.UTF8);
            JObject o = JObject.Parse(sr.ReadToEnd());

            JToken jfts = o["features"];

            List<JToken> jlst = jfts.ToList<JToken>();
            int jlen = jlst.Count;

            List<Polyline> polyLineList = new List<Polyline>(jlen);
            List<double> densityList = new List<double>(jlen);
            Dictionary<string, List<string>> propDic = new Dictionary<string, List<string>>();

            for (int i = 0; i < jlen; i++)
            {
                string jt1 = (string)jlst[i]["geometry"]["type"];
                if (jt1 == "LineString")
                {
                    try
                    {
                        var poiArr = jlst[i]["geometry"]["coordinates"];
                        var itemProperties = poiArr.Children<JArray>();
                        var ptArray = RecorrectCoordinates(itemProperties, coordX, coordY);

                        //Point3d[] ptArray = new Point3d[itemProperties.Count()];
                        //for (int j = 0; j < itemProperties.Count(); j++)
                        //{
                        //    ptArray[j] = new Point3d(double.Parse(itemProperties.ElementAt(j)[0].ToString()), double.Parse(itemProperties.ElementAt(j)[1].ToString()), 0d);
                        //}

                        Polyline polyLine = new Polyline(ptArray);
                        polyLineList.Add(polyLine);

                        var propertyArr = jlst[i]["geometry"]["coordinates"];
                        JToken gprop = jlst[i]["properties"];
                        var propNum = gprop.Count();
                        if (i == 0)
                        {
                            for (int propCount = 0; propCount < propNum; propCount++)
                            {
                                var tempList = new List<string>();
                                JProperty jpeo = (JProperty)gprop.ElementAt(propCount);
                                var tempStr = jpeo.Name.ToString();
                                propDic.Add(tempStr, tempList);
                                propDic[tempStr].Add(jpeo.Value.ToString());
                            }
                        }
                        else
                        {
                            for (int propCount = 0; propCount < propNum; propCount++)
                            {
                                JProperty jpeo = (JProperty)gprop.ElementAt(propCount);
                                var tempStr = jpeo.Name.ToString();

                                propDic[tempStr].Add(jpeo.Value.ToString());
                            }
                        }

                        ParseJsonProperty(0, gprop, densityList);
                        //ParseJsonProperty(1, gprop, siteTypeList);
                        //ParseJsonProperty(2, gprop, mixRList);
                        //ParseJsonProperty(3, gprop, FARList);
                        //ParseJsonProperty(4, gprop, BStyleList);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else { }
            }
            outPolyLineList = polyLineList;

            outDensityList = propDic["Density"];
            outSiteTypeList = propDic["siteType"];
            outMixRList = propDic["MixRatio"];
            outFARList = propDic["FAR"];
            outBStyleList = propDic["BuildingStyle"];
        }
        public static void ReadJson(string inputFile, out List<Polyline> outPolyLineList)
        {
            StreamReader sr = new StreamReader(inputFile, Encoding.UTF8);
            JObject o = JObject.Parse(sr.ReadToEnd());

            JToken jfts = o["features"];

            List<JToken> jlst = jfts.ToList<JToken>();

            List<Polyline> polyLineList = new List<Polyline>();

            for (int i = 0; i < jlst.Count; i++)
            {
                string jt1 = (string)jlst[i]["geometry"]["type"];
                if (jt1 == "LineString")
                {
                    try
                    {
                        var poiArr = jlst[i]["geometry"]["coordinates"];
                        var itemProperties = poiArr.Children<JArray>();
                        Point3d[] ptArray = new Point3d[itemProperties.Count()];
                        for (int j = 0; j < itemProperties.Count(); j++)
                        {
                            ptArray[j] = new Point3d(double.Parse(itemProperties.ElementAt(j)[0].ToString()), double.Parse(itemProperties.ElementAt(j)[1].ToString()), 0d);
                        }
                        Polyline polyLine = new Polyline(ptArray);
                        polyLineList.Add(polyLine);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else { }
            }
            outPolyLineList = polyLineList;
        }

        public static void ReadJson(string inputFile, out List<UrbanXLine> outUrbanLineList)
        {
            StreamReader sr = new StreamReader(inputFile, Encoding.UTF8);
            JObject o = JObject.Parse(sr.ReadToEnd());

            JToken jfts = o["features"];

            List<JToken> jlst = jfts.ToList<JToken>();

            List<UrbanXLine> polyLineList = new List<UrbanXLine>();

            for (int i = 0; i < jlst.Count; i++)
            {
                string jt1 = (string)jlst[i]["geometry"]["type"];
                if (jt1 == "LineString")
                {
                    try
                    {
                        var poiArr = jlst[i]["geometry"]["coordinates"];
                        var itemProperties = poiArr.Children<JArray>();
                        UrbanXPoint[] ptArray = new UrbanXPoint[2];
                        for (int j = 0; j < itemProperties.Count(); j++)
                        {
                            ptArray[j] = new UrbanXPoint(double.Parse(itemProperties.ElementAt(j)[0].ToString()), double.Parse(itemProperties.ElementAt(j)[1].ToString()));
                        }
                        UrbanXLine polyLine = new UrbanXLine(ptArray[0], ptArray[1]);
                        polyLineList.Add(polyLine);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else { }
            }
            outUrbanLineList = polyLineList;
        }
        public static void ReadJson(string inputFile, out List<Polyline> outPolyLineList, out double coordX, out double coordY)
        {
            StreamReader sr = new StreamReader(inputFile, Encoding.UTF8);
            JObject o = JObject.Parse(sr.ReadToEnd());

            JToken jfts = o["features"];

            List<JToken> jlst = jfts.ToList<JToken>();

            List<Polyline> polyLineList = new List<Polyline>();

            //提取第一个点
            var poiArrTemp = jlst[0]["geometry"]["coordinates"];
            var itemPropertiesTemp = poiArrTemp.Children<JArray>();

            var tempPtX = double.Parse(itemPropertiesTemp.ElementAt(0)[0].ToString());
            var tempPtY = double.Parse(itemPropertiesTemp.ElementAt(0)[1].ToString());

            coordX = tempPtX;
            coordY = tempPtY;

            //创建线段
            for (int i = 0; i < jlst.Count; i++)
            {
                string jt1 = (string)jlst[i]["geometry"]["type"];
                if (jt1 == "LineString")
                {
                    try
                    {
                        var poiArr = jlst[i]["geometry"]["coordinates"];
                        var itemProperties = poiArr.Children<JArray>();

                        var ptArray = RecorrectCoordinates(itemProperties, tempPtX, tempPtY);

                        Polyline polyLine = new Polyline(ptArray);
                        polyLineList.Add(polyLine);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else { }
            }
            outPolyLineList = polyLineList;
        }
        public static void ReadJson(string inputFile, out List<Polyline> outPolyLineList, out List<double> outssValueList, out double coordX, out double coordY)
        {
            StreamReader sr = new StreamReader(inputFile, Encoding.UTF8);
            JObject o = JObject.Parse(sr.ReadToEnd());

            #region 新加内容
            JToken jcoords = o["correctCoordinates"];
            JToken gcoordProp = jcoords["properties"];

            ParseJsonProperty(0, gcoordProp, out coordX);
            ParseJsonProperty(1, gcoordProp, out coordY);
            #endregion

            JToken jfts = o["features"];

            List<JToken> jlst = jfts.ToList<JToken>();
            int jlen = jlst.Count;

            List<Polyline> polyLineList = new List<Polyline>(jlen);
            List<double> ssValueList = new List<double>(jlen);
            for (int i = 0; i < jlen; i++)
            {
                string jt1 = (string)jlst[i]["geometry"]["type"];
                if (jt1 == "LineString")
                {
                    try
                    {
                        var poiArr = jlst[i]["geometry"]["coordinates"];
                        var itemProperties = poiArr.Children<JArray>();

                        var ptArray = RecorrectCoordinates(itemProperties, coordX, coordY);

                        //Point3d[] ptArray = new Point3d[itemProperties.Count()];
                        //for (int j = 0; j < itemProperties.Count(); j++)
                        //{
                        //    ptArray[j] = new Point3d(double.Parse(itemProperties.ElementAt(j)[0].ToString()), double.Parse(itemProperties.ElementAt(j)[1].ToString()), 0d);
                        //}
                        Polyline polyLine = new Polyline(ptArray);
                        polyLineList.Add(polyLine);

                        var propertyArr = jlst[i]["geometry"]["coordinates"];
                        JToken gprop = jlst[i]["properties"];

                        ParseJsonProperty(0, gprop, ssValueList);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else { }
            }
            outPolyLineList = polyLineList;
            outssValueList = ssValueList;
        }
        private static void ParseJsonProperty(int index, JToken gprop, List<double> dList)
        {
            JProperty jpeo = (JProperty)gprop.ElementAt(index);
            var tempStr = double.Parse(jpeo.Value.ToString());
            dList.Add(tempStr);
        }
        private static void ParseJsonProperty(int index, JToken gprop, List<int> SList)
        {
            JProperty jpeo = (JProperty)gprop.ElementAt(index);
            var tempStr = int.Parse(jpeo.Value.ToString());
            SList.Add(tempStr);
        }

        private static void ParseJsonProperty(int index, JToken gprop, out double result)
        {
            JProperty jpeo = (JProperty)gprop.ElementAt(index);
            result = double.Parse(jpeo.Value.ToString());
        }

        private static Point3d[] RecorrectCoordinates(JEnumerable<JArray> itemProperties, double tempPtX, double tempPtY)
        {
            Point3d[] ptArray = new Point3d[itemProperties.Count()];
            for (int j = 0; j < itemProperties.Count(); j++)
            {
                ptArray[j] = new Point3d(double.Parse(itemProperties.ElementAt(j)[0].ToString()) - tempPtX, double.Parse(itemProperties.ElementAt(j)[1].ToString()) - tempPtY, 0d);
            }
            return ptArray;
        }

        private static bool IsNumberic(string str)
        {
            double vsNum;
            bool isNum;
            isNum = double.TryParse(str, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out vsNum);
            return isNum;
        }
    }
}
