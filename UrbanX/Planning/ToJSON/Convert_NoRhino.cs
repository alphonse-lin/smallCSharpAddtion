using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using System.Linq;

using UrbanX.DataStructures.Geometry;

namespace UrbanX.Planning.ToJSON
{
    public class Convert_NoRhino
    {
        private static UrbanXPoint[] ConvertLine2Point(UrbanXLine singleLine)
        {
            UrbanXPoint[] resultPts = new UrbanXPoint[2];
            resultPts[0] = singleLine.From;
            resultPts[1] = singleLine.To;
            return resultPts;
        }

        public static string CreateJson_OutLines(IEnumerable<UrbanXLine> urbanXLineList, IEnumerable<double> countTimes)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < urbanXLineList.Count(); i++)
            {
                var singleLine = urbanXLineList.ElementAt(i);
                var ptArray = ConvertLine2Point(singleLine);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length; j++)
                {
                    JArray ptValue = new JArray(ptArray[j].X, ptArray[j].Y);
                    ptValueList.Add(ptValue);
                }

                JProperty jcoord = new JProperty("coordinates", ptValueList);
                JProperty jpolyLine = new JProperty("type", "LineString");
                JProperty jgomtry = new JProperty("geometry", new JObject(jpolyLine, jcoord));

                //add properties
                JProperty j_ssValue = new JProperty("exposureRate", countTimes.ElementAt(i));
                JProperty jprots = new JProperty("properties", new JObject(j_ssValue));

                JObject jtfea = new JObject(new JProperty("type", "Feature"), jprots, jgomtry);
                jfeslst.Add(jtfea);
            }
            JProperty jname = new JProperty("name", "ExposureGeoJSON");

            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrProProjection);

            JProperty jcrs = new JProperty("crs", jcrs_Pro);
            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcrs, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }

        public static string CreateJson_OutLines(IEnumerable<UrbanXLine> urbanXLineList)
        {
            string JsonFile = "";
            List<JObject> jfeslst = new List<JObject>();
            for (int i = 0; i < urbanXLineList.Count(); i++)
            {
                var singleLine = urbanXLineList.ElementAt(i);
                var ptArray = ConvertLine2Point(singleLine);

                List<JArray> ptValueList = new List<JArray>();
                for (int j = 0; j < ptArray.Length; j++)
                {
                    JArray ptValue = new JArray(ptArray[j].X, ptArray[j].Y);
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
            JProperty jname = new JProperty("name", "ExposureGeoJSON");

            JProperty jcrProProjection = new JProperty("name", "urn:ogc:def:crs:EPSG::32650");
            JObject jcrs_Pro = new JObject(new JProperty("type", "name"), jcrProProjection);

            JProperty jcrs = new JProperty("crs", jcrs_Pro);
            JProperty jfs = new JProperty("features", jfeslst);

            JObject json = new JObject(new JProperty("type", "FeatureCollection"), jname, jcrs, jfs);
            JsonFile = json.ToString();

            return JsonFile;
        }
        public static void ExportJsonFile(string jsonString, string directory, GeometryType gType)
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
                case GeometryType.BuildingOutLines:
                    path = Path.Combine(directory, "buildingOutlines.geojson");
                    break;
                case GeometryType.RoadPoints:
                    path = Path.Combine(directory, "roadPoints.geojson");
                    break;
                case GeometryType.RoadPoints_Rays:
                    path = Path.Combine(directory, "roadPointsRays.geojson");
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

        public static void ReadJson(string inputFile, out List<UrbanXLine> outUrbanXLineList)
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
            outUrbanXLineList = polyLineList;
        }

        public static void ReadJson(string inputFile, out List<UrbanXPoint> outUrbanXPointList)
        {
            StreamReader sr = new StreamReader(inputFile, Encoding.UTF8);
            JObject o = JObject.Parse(sr.ReadToEnd());

            JToken jfts = o["features"];

            List<JToken> jlst = jfts.ToList<JToken>();

            List<UrbanXPoint> urbanXptList = new List<UrbanXPoint>(jlst.Count);

            for (int i = 0; i < jlst.Count; i++)
            {
                string jt1 = (string)jlst[i]["geometry"]["type"];
                if (jt1 == "Point")
                {
                    try
                    {
                        var poiArr = jlst[i]["geometry"]["coordinates"];
                        var itemProperties = poiArr.Parent;
                        urbanXptList.Add(new UrbanXPoint(double.Parse(itemProperties.ElementAt(0)[0].ToString()), double.Parse(itemProperties.ElementAt(0)[1].ToString())));
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else { }
            }
            outUrbanXPointList = urbanXptList;
        }
    }
}
