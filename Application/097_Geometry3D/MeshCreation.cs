using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using g3;
using gs;
using System.IO;
using UrbanXX.IO.GeoJSON;
using NTS = NetTopologySuite;
using System.Drawing;
using NetTopologySuite.Features;
using Urbanx.Application.Geometry.Extension;
using Rh = Rhino.Geometry;
using System.Collections.Concurrent;

namespace UrbanX.Application.Geometry
{
    public enum VisDataType
    {
        TotalVisArea,
        VisRatio,
        normalizedVisRatio

    }
    public enum BoundaryModes
    {
        FreeBoundaries,
        FixedBoundaries,
        ConstrainedBoundaries
    }
    public class MeshCreation
    {

        #region 000_Basic Function
        public static Vector3d[][] ReadJsonData(string jsonFilePath, string baseHeightAttribute, string heightAttribute, out double[] heightCollection, out double[]baseHeightCollection)
        {
            StreamReader sr = File.OpenText(jsonFilePath);
            var feactureCollection = GeoJsonReader.GetFeatureCollectionFromJson(sr.ReadToEnd());
            Vector3d[][] vectorResult = new Vector3d[feactureCollection.Count][];
            double[] heightResult = new double[feactureCollection.Count];
            double[] baseHeightResult = new double[feactureCollection.Count];

            for (int i = 0; i < feactureCollection.Count; i++)
            {
                //读取数据
                var jsonDic = feactureCollection[i].Geometry;
                int geoCount = jsonDic.Coordinates.Length;
                vectorResult[i] = new Vector3d[geoCount];
                var jsonDic_baseHeight = feactureCollection[i].Attributes[baseHeightAttribute];
                var tempBaseHeightResult = double.Parse(jsonDic_baseHeight.ToString());

                for (int num = 0; num < jsonDic.Coordinates.Length; num++)
                {
                    vectorResult[i][num] = new Vector3d(jsonDic.Coordinates[num].X, jsonDic.Coordinates[num].Y, tempBaseHeightResult);
                }
                var jsonDic_height = feactureCollection[i].Attributes[heightAttribute];

                heightResult[i] = double.Parse(jsonDic_height.ToString());
                baseHeightResult[i] = tempBaseHeightResult;
            }

            heightCollection = heightResult;
            baseHeightCollection = baseHeightResult;
            return vectorResult;
        }

        public static Vector3d[][] ReadJsonData(string jsonFilePath, string baseHeightAttribute, string heightAttribute, out double[] heightCollection, out double[][] envelopeCollection)
        {
            StreamReader sr = File.OpenText(jsonFilePath);
            var feactureCollection = GeoJsonReader.GetFeatureCollectionFromJson(sr.ReadToEnd());
            Vector3d[][] vectorResult = new Vector3d[feactureCollection.Count][];
            double[] heightResult = new double[feactureCollection.Count];
            double[][] envelopeList = new double[feactureCollection.Count][];

            for (int i = 0; i < feactureCollection.Count; i++)
            {
                //读取数据
                var jsonDic = feactureCollection[i].Geometry;
                int geoCount = jsonDic.Coordinates.Length;

                vectorResult[i] = new Vector3d[geoCount];
                envelopeList[i] = new double[4] {
                    jsonDic.EnvelopeInternal.MinX,
                    jsonDic.EnvelopeInternal.MinY,
                    jsonDic.EnvelopeInternal.MaxX,
                    jsonDic.EnvelopeInternal.MaxY,
                };

                var jsonDic_baseHeight = feactureCollection[i].Attributes[baseHeightAttribute];
                var baseHeightResult = double.Parse(jsonDic_baseHeight.ToString());

                for (int num = 0; num < jsonDic.Coordinates.Length; num++)
                {
                    vectorResult[i][num] = new Vector3d(jsonDic.Coordinates[num].X, jsonDic.Coordinates[num].Y, baseHeightResult);
                }
                var jsonDic_height = feactureCollection[i].Attributes[heightAttribute];

                heightResult[i] = double.Parse(jsonDic_height.ToString());
            }

            heightCollection = heightResult;
            envelopeCollection = envelopeList;
            return vectorResult;
        }

        public static NTS.Geometries.Point[] ReadJsonData(string jsonFilePath, string attribute, out double[] attributeData)
        {
            StreamReader sr = File.OpenText(jsonFilePath);
            var feactureCollection = GeoJsonReader.GetFeatureCollectionFromJson(sr.ReadToEnd());
            NTS.Geometries.Point[] ptResult = new NTS.Geometries.Point[feactureCollection.Count];
            double[] tempResult = new double[feactureCollection.Count];

            for (int i = 0; i < feactureCollection.Count; i++)
            {
                //读取数据
                var jsonDic = feactureCollection[i].Geometry;
                ptResult[i] = new NTS.Geometries.Point(jsonDic.Coordinate.X, jsonDic.Coordinate.Y, jsonDic.Coordinate.Z);
                var jsonDic_data = feactureCollection[i].Attributes[attribute];

                tempResult[i] = double.Parse(jsonDic_data.ToString());
            }

            attributeData = tempResult;
            return ptResult;
        }

        public static NTS.Geometries.Point[] ReadJsonData(string jsonFilePath, string attribute, out Dictionary<NTS.Geometries.Point, double> attributeData)
        {
            StreamReader sr = File.OpenText(jsonFilePath);
            var feactureCollection = GeoJsonReader.GetFeatureCollectionFromJson(sr.ReadToEnd());
            NTS.Geometries.Point[] ptResult = new NTS.Geometries.Point[feactureCollection.Count];
            double[] tempResult = new double[feactureCollection.Count];
            Dictionary<NTS.Geometries.Point, double> geoDic = new Dictionary<NTS.Geometries.Point, double>();

            for (int i = 0; i < feactureCollection.Count; i++)
            {
                //读取数据
                var jsonDic = feactureCollection[i].Geometry;
                ptResult[i] = new NTS.Geometries.Point(jsonDic.Coordinate.X, jsonDic.Coordinate.Y, jsonDic.Coordinate.Z);
                var jsonDic_data = feactureCollection[i].Attributes[attribute];
                geoDic.Add(ptResult[i], double.Parse(jsonDic_data.ToString()));
            }

            attributeData = geoDic;
            return ptResult;
        }

        public static Vector3d[][] ReadJsonData(string jsonFilePath)
        {
            StreamReader sr = File.OpenText(jsonFilePath);
            var feactureCollection = GeoJsonReader.GetFeatureCollectionFromJson(sr.ReadToEnd());
            Vector3d[][] vectorResult = new Vector3d[feactureCollection.Count][];
            double[] heightResult = new double[feactureCollection.Count];

            for (int i = 0; i < feactureCollection.Count; i++)
            {
                //读取数据
                var jsonDic = feactureCollection[i].Geometry;
                int geoCount = jsonDic.Coordinates.Length;
                vectorResult[i] = new Vector3d[geoCount];
                for (int num = 0; num < jsonDic.Coordinates.Length; num++)
                {
                    vectorResult[i][num] = new Vector3d(jsonDic.Coordinates[num].X, jsonDic.Coordinates[num].Y, 0);
                }
            }
            return vectorResult;
        }

        public static Vector2d[][] ReadJsonData2D(string jsonFilePath)
        {
            StreamReader sr = File.OpenText(jsonFilePath);
            var feactureCollection = GeoJsonReader.GetFeatureCollectionFromJson(sr.ReadToEnd());
            Vector2d[][] vectorResult = new Vector2d[feactureCollection.Count][];
            double[] heightResult = new double[feactureCollection.Count];

            for (int i = 0; i < feactureCollection.Count; i++)
            {
                //读取数据
                var jsonDic = feactureCollection[i].Geometry;
                int geoCount = jsonDic.Coordinates.Length;
                vectorResult[i] = new Vector2d[geoCount];
                for (int num = 0; num < jsonDic.Coordinates.Length; num++)
                {
                    vectorResult[i][num] = new Vector2d(jsonDic.Coordinates[num].X, jsonDic.Coordinates[num].Y);
                }
            }
            return vectorResult;
        }

        public static FeatureCollection BuildFeatureCollection(NTS.Geometries.Geometry[] geosInfo, double[] area)
        {
            var fc = new FeatureCollection();
            for (int i = 0; i < geosInfo.Length; i++)
            {
                AttributesTable att = new AttributesTable
                {
                    { "meshId",i},
                    { "Area", area[i]}
                };
                Feature f = new Feature(geosInfo[i], att);
                fc.Add(f);
            }
            return fc;
        }

        public static FeatureCollection BuildFeatureCollection(NTS.Geometries.Geometry[] geosInfo)
        {
            var fc = new FeatureCollection();
            for (int i = 0; i < geosInfo.Length; i++)
            {
                AttributesTable att = new AttributesTable
                {
                };
                Feature f = new Feature(geosInfo[i], att);
                fc.Add(f);
            }
            return fc;
        }

        public static void ExportGeoJSON(FeatureCollection fc, string outputPath)
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            GeoJsonWriter writer = new GeoJsonWriter();
            var outputString = writer.Write(fc);
            using (StreamWriter sw = new StreamWriter(outputPath, true))
            {
                sw.Write(outputString);
                sw.Flush();
            }
        }

        public static void ExportMeshAsObj(string path, DMesh3 mesh, bool color = false)
        {
            WriteOptions writeOption = new WriteOptions()
            {
                bWriteBinary = false,
                bPerVertexNormals = false,
                bPerVertexColors = color,
                bWriteGroups = false,
                bPerVertexUVs = false,
                bCombineMeshes = false,
                bWriteMaterials = false,
                ProgressFunc = null,
                //MaterialFilePath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geometryTest\exportColor2.mtl",
                RealPrecisionDigits = 15       // double
                                               //RealPrecisionDigits = 7        // float
            };
            IOWriteResult result = StandardMeshWriter.WriteFile(path, new List<WriteMesh>() { new WriteMesh(mesh) }, writeOption);
        }

        public static void ExportMeshAsStl(string path, DMesh3 mesh, bool color = false)
        {
            WriteOptions writeOption = new WriteOptions()
            {
                bWriteBinary = true,
                bPerVertexNormals = false,
                bPerVertexColors = color,
                bWriteGroups = false,
                bPerVertexUVs = false,
                bCombineMeshes = false,
                bWriteMaterials = false,
                ProgressFunc = null,
                //MaterialFilePath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geometryTest\exportColor2.mtl",
                RealPrecisionDigits = 15       // double
                                               //RealPrecisionDigits = 7        // float
            };
            IOWriteResult result = StandardMeshWriter.WriteFile(path, new List<WriteMesh>() { new WriteMesh(mesh) }, writeOption);
        }

        public static DMesh3 ImportMesh(string path)
        {
            return StandardMeshReader.ReadMesh(path);
        }
        public static void InitiateColor(DMesh3 mesh)
        {
            mesh.EnableVertexColors(new Colorf(Colorf.White));
        }

        public static DMesh3 ApplyColor(DMesh3 mesh, Colorf originColor, Colorf DestnationColor)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            float meshCount = meshIn.VertexCount;

            for (int i = 0; i < meshCount; i++)
            {
                var temp_color = Colorf.Lerp(originColor, DestnationColor, i / meshCount);
                meshIn.SetVertexColor(i, temp_color);
            }
            return meshIn;
        }

        public static DMesh3 ApplyColor(DMesh3 mesh, Colorf originColor, Colorf DestnationColor, float meshCount, Func<float, float> singleCount)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            for (int i = 0; i < meshCount; i++)
            {
                var temp_color = Colorf.Lerp(originColor, DestnationColor, singleCount(i));
                meshIn.SetVertexColor(i, temp_color);
            }
            return meshIn;
        }

        #endregion

        #region 001_Generating Mesh
        public static bool CreateMesh(IEnumerable<Vector3f> vertices, int[] triangles, List<Vector3f> normals, out DMesh3 meshResult)
        {
            DMesh3 mesh = DMesh3Builder.Build(vertices, triangles, normals);
            meshResult = mesh;
            return mesh.CheckValidity();
        }
        public static void CreateMesh(IEnumerable<Vector3d> vertices, int[] triangles, out DMesh3 meshResult)
        {
            List<Vector3d> normals = new List<Vector3d>();
            foreach (var item in vertices)
                normals.Add(Vector3d.AxisZ);

            DMesh3 mesh = DMesh3Builder.Build(vertices, triangles, normals);
            meshResult = mesh;

        }

        public static DMesh3 ExtrudeMeshFromPt(Vector3d[][] OriginalData, double[] height)
        {
            DMesh3 meshCollection = new DMesh3();
            for (int i = 0; i < OriginalData.Length; i++)
            {
                var meshSrf = BoundarySrfFromPts(OriginalData[i]);
                var meshExtruded = ExtrudeMeshFromMesh(meshSrf, height[i]);
                MeshEditor.Append(meshCollection, meshExtruded);
            }
            return meshCollection;
        }

        public static DMesh3 ExtrudeMeshFromPt(Vector3d[][] OriginalData, double[] height, out Dictionary<NTS.Geometries.Point, double> centerPtDic)
        {
            Dictionary<NTS.Geometries.Point, double> temp_centerPtDic = new Dictionary<NTS.Geometries.Point, double>();
            DMesh3 meshCollection = new DMesh3();
            for (int i = 0; i < OriginalData.Length; i++)
            {
                var meshSrf = BoundarySrfFromPts(OriginalData[i], out NTS.Geometries.Point centerPt);
                var meshExtruded = ExtrudeMeshFromMesh(meshSrf, height[i]);
                MeshEditor.Append(meshCollection, meshExtruded);
                var triAreaList = new List<double>(meshExtruded.TriangleCount);
                for (int j = 0; j < meshExtruded.TriangleCount; j++)
                {
                    triAreaList.Add(meshExtruded.GetTriArea(j));
                }
                var tempArea = triAreaList.Sum();

                if (temp_centerPtDic.ContainsKey(centerPt))
                {
                    var tempAreaInDic = temp_centerPtDic[centerPt];
                    temp_centerPtDic[centerPt] = tempArea + tempAreaInDic;
                }
                else
                    temp_centerPtDic.Add(centerPt, tempArea);
            }

            centerPtDic = temp_centerPtDic;
            return meshCollection;
        }

        public static DMesh3 ExtrudeMeshFromPtMinusTopBtn(Vector3d[][] OriginalData, double[] height, out Dictionary<NTS.Geometries.Point, double> centerPtDic)
        {
            Dictionary<NTS.Geometries.Point, double> temp_centerPtDic = new Dictionary<NTS.Geometries.Point, double>();
            DMesh3 meshCollection = new DMesh3();
            for (int i = 0; i < OriginalData.Length; i++)
            {
                var meshSrf = BoundarySrfFromPts(OriginalData[i], out NTS.Geometries.Point centerPt, out double meshArea);
                //var meshExtruded = ExtrudeMeshFromMesh(meshSrf, height[i]); 
                var meshExtruded = ExtrudeMeshEdge(meshSrf, height[i]); 

                MeshEditor.Append(meshCollection, meshExtruded);
                var triAreaList = new List<double>(meshExtruded.TriangleCount);
                for (int j = 0; j < meshExtruded.TriangleCount; j++)
                {
                    triAreaList.Add(meshExtruded.GetTriArea(j));
                }
                var tempArea = triAreaList.Sum() - meshArea * 2;

                if (temp_centerPtDic.ContainsKey(centerPt))
                {
                    var tempAreaInDic = temp_centerPtDic[centerPt];
                    temp_centerPtDic[centerPt] = tempArea + tempAreaInDic;
                }
                else
                    temp_centerPtDic.Add(centerPt, tempArea);
            }

            centerPtDic = temp_centerPtDic;
            return meshCollection;
        }

        public static DMesh3 ExtrudeMeshFromPtMinusTopBtn(Vector3d[][] OriginalData, double[] height, double[] baeHeight, out Dictionary<NTS.Geometries.Point, double> centerPtDic, out DCurve3[][] edges)
        {
            Dictionary<NTS.Geometries.Point, double> temp_centerPtDic = new Dictionary<NTS.Geometries.Point, double>();
            DMesh3 meshCollection = new DMesh3();
            DCurve3[][] meshEdge = new DCurve3[OriginalData.Length][];
            for (int i = 0; i < OriginalData.Length; i++)
            {
                var meshSrf = BoundarySrfFromPts(OriginalData[i], out NTS.Geometries.Point centerPt, out double meshArea);
                //var meshExtruded = ExtrudeMeshFromMesh(meshSrf, height[i]); 
                var meshExtruded = ExtrudeMeshEdge(meshSrf, height[i]);
                meshEdge[i] = ExtractEdge(OriginalData[i],height[i], baeHeight[i]);

                MeshEditor.Append(meshCollection, meshExtruded);
                var triAreaList = new List<double>(meshExtruded.TriangleCount);
                for (int j = 0; j < meshExtruded.TriangleCount; j++)
                {
                    triAreaList.Add(meshExtruded.GetTriArea(j));
                }
                var tempArea = triAreaList.Sum() - meshArea * 2;

                if (temp_centerPtDic.ContainsKey(centerPt))
                {
                    var tempAreaInDic = temp_centerPtDic[centerPt];
                    temp_centerPtDic[centerPt] = tempArea + tempAreaInDic;
                }
                else
                    temp_centerPtDic.Add(centerPt, tempArea);
            }
            edges = meshEdge;
            centerPtDic = temp_centerPtDic;
            return meshCollection;
        }

        public static DMesh3[] ExtrudeMeshListFromPtMinusTopBtn(Vector3d[][] OriginalData, double[] height,double[] baseHeight, out Dictionary<NTS.Geometries.Point, double> centerPtDic, out DCurve3[][] edges)
        {
            Dictionary<NTS.Geometries.Point, double> temp_centerPtDic = new Dictionary<NTS.Geometries.Point, double>();
            DMesh3[] meshCollection = new DMesh3[OriginalData.Length];
            DCurve3[][] meshEdge = new DCurve3[OriginalData.Length][];
            for (int i = 0; i < OriginalData.Length; i++)
            {
                DMesh3 tempMesh = new DMesh3();
                var meshSrf = BoundarySrfFromPts(OriginalData[i], out NTS.Geometries.Point centerPt, out double meshArea);
                //var meshExtruded = ExtrudeMeshFromMesh(meshSrf, height[i]); 
                var meshExtruded = ExtrudeMeshEdge(meshSrf, height[i]);
                meshEdge[i] = ExtractEdge(OriginalData[i], baseHeight[i], height[i]);

                MeshEditor.Append(tempMesh, meshExtruded);
                meshCollection[i]= tempMesh;
                var triAreaList = new List<double>(meshExtruded.TriangleCount);
                for (int j = 0; j < meshExtruded.TriangleCount; j++)
                {
                    triAreaList.Add(meshExtruded.GetTriArea(j));
                }
                var tempArea = triAreaList.Sum();

                if (temp_centerPtDic.ContainsKey(centerPt))
                {
                    var tempAreaInDic = temp_centerPtDic[centerPt];
                    temp_centerPtDic[centerPt] = tempArea + tempAreaInDic;
                }
                else
                { temp_centerPtDic.Add(centerPt, tempArea); }
            }
            edges = meshEdge;
            centerPtDic = temp_centerPtDic;
            return meshCollection;
        }
        public static DMesh3 ExtrudeMeshFromPt(Vector3d[][] OriginalData, double height = 10d)
        {
            DMesh3 meshCollection = new DMesh3();
            for (int i = 0; i < OriginalData.Length; i++)
            {
                var meshSrf = BoundarySrfFromPts(OriginalData[i]);
                var meshExtruded = ExtrudeMeshFromMesh(meshSrf, height);

                MeshEditor.Append(meshCollection, meshExtruded);
            }
            return meshCollection;
        }

        public static DMesh3 ExtrudeRemeshMeshFromPt(Vector3d[][] OriginalData, double height = 10d, double targetEdgeLength = 1d, double smoothSpeedT = 0.5d)
        {
            DMesh3 meshCollection = new DMesh3();
            for (int i = 0; i < OriginalData.Length; i++)
            {
                var meshSrf = BoundarySrfFromPts(OriginalData[i]);
                var meshExtruded = ExtrudeMeshFromMesh(meshSrf, height);
                var meshRemesher = MeshCreation.SimpleRemesher(meshExtruded, targetEdgeLength, smoothSpeedT);

                MeshEditor.Append(meshCollection, meshRemesher);
            }
            return meshCollection;
        }

        public static DMesh3 ExtrudeRemeshMeshFromPt(Vector3d[][] OriginalData, double[] height, double targetEdgeLength = 1d, double smoothSpeedT = 0.5d)
        {
            DMesh3 meshCollection = new DMesh3();
            for (int i = 0; i < OriginalData.Length; i++)
            {
                var meshSrf = BoundarySrfFromPts(OriginalData[i]);
                var meshExtruded = ExtrudeMeshFromMesh(meshSrf, height[i]);
                var meshRemesher = MeshCreation.SimpleRemesher(meshExtruded, targetEdgeLength, smoothSpeedT);
                MeshEditor.Append(meshCollection, meshRemesher);
            }
            return meshCollection;
        }
        
        /// <summary>
        /// Create mesh from a list of points
        /// </summary>
        /// <param name="vectorListInput"></param>
        /// <param name="indicesResult"></param>
        /// <returns></returns>
        public static DMesh3 BoundarySrfFromPts(Vector3d[] vectorListInput)
        {
            // Use the triangulator to get indices for creating triangles
            var vectorList = new Vector3d[vectorListInput.Length - 1];
            for (int i = 0; i < vectorListInput.Length - 1; i++)
                vectorList[i] = vectorListInput[i];

            Triangulator tri = new Triangulator(vectorList);
            int[] indices = tri.Triangulate();
            CreateMesh(vectorList, indices, out DMesh3 meshResult);
            return meshResult;
        }
        public static DMesh3 BoundarySrfFromPts(Vector3d[] vectorListInput, out Vector3d centerPt)
        {
            // Use the triangulator to get indices for creating triangles
            var vectorList = new Vector3d[vectorListInput.Length - 1];
            for (int i = 0; i < vectorListInput.Length - 1; i++)
                vectorList[i] = vectorListInput[i];

            Triangulator tri = new Triangulator(vectorList);
            int[] indices = tri.Triangulate();
            CreateMesh(vectorList, indices, out DMesh3 meshResult);
            centerPt = meshResult.CachedBounds.Center;
            return meshResult;
        }
        public static DMesh3 BoundarySrfFromPts(Vector3d[] vectorListInput, out NTS.Geometries.Point centerPt)
        {
            // Use the triangulator to get indices for creating triangles
            var vectorList = new Vector3d[vectorListInput.Length - 1];
            for (int i = 0; i < vectorListInput.Length - 1; i++)
                vectorList[i] = vectorListInput[i];

            Triangulator tri = new Triangulator(vectorList);
            int[] indices = tri.Triangulate();
            CreateMesh(vectorList, indices, out DMesh3 meshResult);
            centerPt = new NTS.Geometries.Point(meshResult.CachedBounds.Center.x, meshResult.CachedBounds.Center.y);
            return meshResult;
        }
        public static DMesh3 BoundarySrfFromPts(Vector3d[] vectorListInput, out NTS.Geometries.Point centerPt, out double meshArea)
        {
            // Use the triangulator to get indices for creating triangles
            var vectorList = new Vector3d[vectorListInput.Length - 1];
            for (int i = 0; i < vectorListInput.Length - 1; i++)
                vectorList[i] = vectorListInput[i];

            Triangulator tri = new Triangulator(vectorList);
            int[] indices = tri.Triangulate();
            CreateMesh(vectorList, indices, out DMesh3 meshResult);
            centerPt = new NTS.Geometries.Point(meshResult.CachedBounds.Center.x, meshResult.CachedBounds.Center.y);

            var triAreaList = new List<double>(meshResult.TriangleCount);
            for (int j = 0; j < meshResult.TriangleCount; j++)
                triAreaList.Add(meshResult.GetTriArea(j));
            meshArea = triAreaList.Sum();
            return meshResult;
        }
        public static void BoundarySrfFromPts(Vector3d[] vectorListInput, out int[] indicesResult)
        {
            // Use the triangulator to get indices for creating triangles
            var vectorList = new Vector3d[vectorListInput.Length - 1];
            for (int i = 0; i < vectorListInput.Length - 1; i++)
                vectorList[i] = vectorListInput[i];

            Triangulator tri = new Triangulator(vectorList);
            indicesResult = tri.Triangulate();
        }

        /// <summary>
        /// extrude a boundary loop of mesh and connect w/ triangle strip
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static DMesh3 ExtrudeMeshEdge(DMesh3 mesh, double height)
        {
            var meshResult = mesh;
            var removeCount = mesh.TriangleCount;

            var removeIndex = new int[removeCount];
            for (int i = 0; i < removeCount; i++)
                removeIndex[i] = i;

            MeshBoundaryLoops loops = new MeshBoundaryLoops(mesh);
            EdgeLoop eLoop = new EdgeLoop(mesh);
            eLoop.Edges = loops[0].Edges;
            eLoop.Vertices = loops[0].Vertices;
            new MeshExtrudeLoop(meshResult, eLoop)
            {
                PositionF = (v, n, vid) => v + height * Vector3d.AxisZ
            }.Extrude();
            var debug = meshResult.Triangles();
            //MeshLoopClosure meshClose = new MeshLoopClosure(mesh, eLoop);
            //meshClose.Close_Flat();
            MeshEditor.RemoveTriangles(meshResult, removeIndex);
            return meshResult;
        }

        /// <summary>
        /// Extrude a boundary Faces of mesh and connect w/ triangle strip
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static DMesh3 ExtrudeMeshFaces(DMesh3 mesh, int[] triangles, double height)
        {
            var meshResult = mesh;
            new MeshExtrudeFaces(meshResult, triangles, true)
            {
                ExtrudedPositionF = ((Func<Vector3d, Vector3f, int, Vector3d>)((v, n, vid) => v + height * Vector3d.AxisZ))
            }.Extrude();
            return meshResult;
        }

        /// <summary>
        /// Extrude mesh  with certain height and connect w/ triangle strip
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static DMesh3 ExtrudeMeshFromMesh(DMesh3 mesh, double height)
        {
            var meshResult = mesh;
            new MeshExtrudeMesh(meshResult)
            {
                ExtrudedPositionF = (v, n, vid) => v + height * Vector3d.AxisZ
            }.Extrude();
            return meshResult;
        }

        #endregion

        #region 002_Remesher
        public static DMesh3 SimpleRemesher(DMesh3 mesh, double targetEdgeLength = 1d, double smoothSpeedT = 0.5d, bool reprojectToInput = false, bool preserve_creases = true, BoundaryModes boundaryMode = BoundaryModes.FixedBoundaries)
        {
            DMesh3 meshIn = new DMesh3(mesh);

            RemesherPro remesh = new RemesherPro(meshIn);
            remesh.SetTargetEdgeLength(targetEdgeLength);
            remesh.SmoothSpeedT = smoothSpeedT;

            if (reprojectToInput)
            {
                var target = MeshProjectionTarget.Auto(meshIn);
                remesh.SetProjectionTarget(target);
            }

            // if we are preserving creases, this will also automatically constrain boundary
            // edges boundary loops/spans. 
            if (preserve_creases)
            {
                if (remesh.Constraints == null)
                    remesh.SetExternalConstraints(new MeshConstraints());

                MeshTopology topo = new MeshTopology(meshIn);
                topo.CreaseAngle = 10d;
                topo.AddRemeshConstraints(remesh.Constraints);

                // replace boundary edge constraints if we want other behaviors
                if (boundaryMode == BoundaryModes.FixedBoundaries)
                    MeshConstraintUtil.FixEdges(remesh.Constraints, meshIn, topo.BoundaryEdges);
            }
            else if (meshIn.CachedIsClosed == false)
            {
                if (remesh.Constraints == null)
                    remesh.SetExternalConstraints(new MeshConstraints());

                if (boundaryMode == BoundaryModes.FreeBoundaries)
                    MeshConstraintUtil.PreserveBoundaryLoops(remesh.Constraints, meshIn);
                else if (boundaryMode == BoundaryModes.FixedBoundaries)
                    MeshConstraintUtil.FixAllBoundaryEdges(remesh.Constraints, meshIn);
                else if (boundaryMode == BoundaryModes.ConstrainedBoundaries)
                    MeshConstraintUtil.FixAllBoundaryEdges_AllowSplit(remesh.Constraints, meshIn, 0);
            }

            remesh.FastestRemesh(25, true);

            // free boundary remesh can leave sliver triangles around the border. clean that up.
            if (meshIn.CachedIsClosed == false && boundaryMode == BoundaryModes.FreeBoundaries)
            {
                MeshEditor.RemoveFinTriangles(meshIn, (mesh, tid) =>
                 {
                     Index3i tv = mesh.GetTriangle(tid);
                     return MathUtil.AspectRatio(mesh.GetVertex(tv.a), mesh.GetVertex(tv.b), mesh.GetVertex(tv.c)) > 2;
                 });
            }

            DMesh3 meshOut = new DMesh3(meshIn, true);
            return meshOut;
        }

        #region 002_Remesher_001_Plankton

        public static PlanktonMesh ReMeshHardEdge( DMesh3 meshIn, int Subdivisions, bool HardBoundaries = true, bool StellateAll = false, double tol = 0.01)
        {
            DMesh3 source = new DMesh3(meshIn);

            var Anchors = new List<Vector3d>();
            var SetCreases = ExtractEdge(source);
            var TargetCreases = new List<DCurve3>();
            var HardCreases = new List<bool>() { true };

            return ReMesh(1, source.g3Mesh2pMesh(),Subdivisions,Anchors,SetCreases,TargetCreases,HardCreases) ;
        }

        public static PlanktonMesh ReMeshHardEdge(DMesh3 meshIn, DCurve3[] meshEdge, int Subdivisions, bool HardBoundaries = true, bool StellateAll = false, double tol = 0.01)
        {
            DMesh3 source = new DMesh3(meshIn);

            var Anchors = new List<Vector3d>();
            var SetCreases = meshEdge.ToList() ;
            var TargetCreases = new List<DCurve3>();
            var HardCreases = new List<bool>() { true};

            return ReMesh(1, source.g3Mesh2pMesh(), Subdivisions, Anchors, SetCreases, TargetCreases, HardCreases);
        }
        public static PlanktonMesh ReMesh(int Algorithm, PlanktonMesh ip, int Subdivisions,  List<Vector3d> Anchors, List<DCurve3> SetCreases, List<DCurve3> TargetCreases,List<bool> HardCreases, bool HardBoundaries=true, bool StellateAll=false, double tol=0.01)
        {
            PlanktonMesh P = new PlanktonMesh(ip);

            // if it's a Loop subdivision, then pre-triangulate, splitting quads and stellating >4 n-gons
            if (Algorithm==1)
            {
                int FaceCount = P.Faces.Count;
                for (int i = 0; i < FaceCount; i++)
                {
                    int[] FaceHEs = P.Faces.GetHalfedges(i);
                    if (FaceHEs.Length == 4 && !StellateAll)
                    {
                        double D0 = P.Vertices[P.Halfedges[FaceHEs[0]].StartVertex].Tog3Pt().Distance(P.Vertices[P.Halfedges[FaceHEs[2]].StartVertex].Tog3Pt());
                        double D1 = P.Vertices[P.Halfedges[FaceHEs[1]].StartVertex].Tog3Pt().Distance(P.Vertices[P.Halfedges[FaceHEs[3]].StartVertex].Tog3Pt());

                        // split face either along the shorter cross corners or at every even one in the case of a regular grid
                        if (D0 < D1|| i%2==0)
                        {
                            P.Faces.SplitFace(FaceHEs[2], FaceHEs[0]);
                        }
                        else
                        {
                            P.Faces.SplitFace(FaceHEs[3], FaceHEs[1]);
                        }
                    }
                    else if (FaceHEs.Length>4 || (StellateAll && FaceHEs.Length==4))
                    {
                        P.Faces.Stellate(i);
                    }
                }
            }

            // ----------------------------------------
            // register anchors, creases and boundaries
            // ----------------------------------------

            List<VertexData> VerticesData = new List<VertexData>();
            var AnchorLookup = Anchors!=null ? new DCurve3(Anchors, false) : new DCurve3();
            var CreaseLookup = new List<List<int>>();

            // if no specification is made for HardCreases, then default to false
            if (HardCreases.Count == 0) HardCreases.Add(false);

            for (int c = 0; c < SetCreases.Count; c++)
            {
                //To Do 尚未添加domain
                if (c+1>TargetCreases.Count)
                { TargetCreases.Add(SetCreases[c]);}

                if (c + 1 > HardCreases.Count)
                    HardCreases.Add(HardCreases.Last());
            }

            var CreaseCheck = new List<int>();
            var debugBoundary = new List<bool>();
            var disList = new double[P.Vertices.Count][];
            var ptList = new Vector3d[P.Vertices.Count][];

            for (int v = 0; v < P.Vertices.Count; v++)
            {
                var ThisPosition = P.Vertices[v].Tog3Pt();

                // key data about each vertex
                bool IsAnchor = false;
                bool IsBoundary = P.Vertices.IsBoundary(v);
                var CreaseIdc = new List<int>();

                debugBoundary.Add(IsBoundary);
                //check if a vertex is on a boundary
                if (IsBoundary)
                {
                    IsAnchor = HardBoundaries;
                    CreaseIdc.Add(-1);
                }

                // check if vertex is proximate to a user-defined anchor
                if (!IsAnchor)
                {
                    int AnchorIdx = g3.CurveUtils.FindNearestIndex(AnchorLookup, ThisPosition);
                    if (AnchorIdx>-1)
                    {
                        if (ThisPosition.Distance(Anchors[AnchorIdx])<tol)
                        {
                            IsAnchor = true;
                        }
                    }
                }


                //check vertices for crease adjacency

                disList[v] = new double[SetCreases.Count];
                ptList[v] = new Vector3d[SetCreases.Count];
                for (int c = 0; c < SetCreases.Count; c++)
                {
                    var tempPt01=SetCreases[c].ClosestPoint(ThisPosition, out int SegIndex01, out double Ct);
                    //disList[v][c]=ThisPosition.Distance(tempPt01);
                    //ptList[v][c]=tempPt01;
                    if (ThisPosition.Distance(tempPt01) <tol)
                    {
                        CreaseIdc.Add(c);
                        if (!IsAnchor)
                        {
                            IsAnchor = HardCreases[c];
                            if (IsAnchor)
                            {
                                var tempPt02=TargetCreases[c].ClosestPoint(ThisPosition, out int SegIndex02, out double Pt);
                                P.Vertices.SetVertex(v, tempPt02);
                            }
                        }
                    }
                }

                CreaseLookup.Add(CreaseIdc);
                if (CreaseIdc.Count > 0) CreaseCheck.Add(v);

                VerticesData.Add(new VertexData(IsAnchor, IsBoundary));
            }

            //find crease neighbours
            foreach (int ThisIdx in CreaseCheck)
            {
                var ThisPosition = P.Vertices[ThisIdx].Tog3Pt();
                VertexData ThisVertex = VerticesData[ThisIdx];

                int[] Neighbours = P.Vertices.GetVertexNeighbours(ThisIdx);
                foreach (int  CreaseIndex in CreaseLookup[ThisIdx])
                {
                    foreach (int NeighbourIdx in Neighbours)
                    {
                        VertexData NeighbourVertex = VerticesData[NeighbourIdx];
                        // if the neighbour shares the crease index and the connection hasn't already been solved
                        if (CreaseLookup[NeighbourIdx].Contains(CreaseIndex)&&!NeighbourVertex.NeighbourIndices.Contains(ThisIdx))
                        {
                            if (P.Halfedges.IsBoundary(P.Halfedges.FindHalfedge(ThisIdx,NeighbourIdx))|| CreaseIndex!=-1)
                            {
                                ThisVertex.CreaseIndices.Add(CreaseIndex);
                                ThisVertex.NeighbourIndices.Add(NeighbourIdx);

                                NeighbourVertex.CreaseIndices.Add(CreaseIndex);
                                NeighbourVertex.NeighbourIndices.Add(ThisIdx);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < Subdivisions; i++)
            {
                P= Subdivision(Algorithm, P, HardBoundaries, ref VerticesData, TargetCreases, HardCreases);
            }

            return P;
        }

        private static PlanktonMesh Subdivision(int Algorithm, PlanktonMesh iP, bool HardBoundaries, ref List<VertexData> VerticesData, List<DCurve3> Creases, List<bool> HardCreases)
        {
            // the starting plankton mesh
            PlanktonMesh P = new PlanktonMesh(iP);
            // intermediary mesh for track indices during splitting
            PlanktonMesh TP = new PlanktonMesh(iP);
            // new mesh for writing new faces from vertex collections
            PlanktonMesh NP = new PlanktonMesh();

            var NewVerts = new List<PlanktonXYZ>();

            // cycle through all even vertices
            for (int v = 0; v < P.Vertices.Count; v++)
            {
                Vector3d NewPosition = new Vector3d();
                bool Solved = false;
                VertexData ThisVertex = VerticesData[v];

                // solve where the vertex is either on a boundary or a crease
                if (ThisVertex.IsAnchor)
                {
                    NewPosition = P.Vertices[v].Tog3Pt();
                    Solved = true;
                }
                else if (ThisVertex.CreaseIndices.Count > 1)
                {
                    int NeighbourCounter = 0;
                    Vector3d NeighbourSum = new Vector3d();

                    // list of hard creases that pull this vertex
                    List<int> PullCreases = new List<int>();

                    // cycle through all of the crease indices
                    for (int cc = 0; cc < ThisVertex.CreaseIndices.Count; cc++)
                    {
                        int NeighbourIndex = ThisVertex.NeighbourIndices[cc];
                        int ThisCreaseIndex = ThisVertex.CreaseIndices[cc];
                        VertexData NeighbourVertex = VerticesData[NeighbourIndex];
                        if (NeighbourVertex.NeighbourIndices.Contains(v))
                        {
                            //ensure reciprocity
                            if ((ThisVertex.IsBoundary && NeighbourVertex.IsBoundary)||
                                (ThisVertex.CreaseIndices.Contains(ThisCreaseIndex) &&NeighbourVertex.CreaseIndices.Contains(ThisCreaseIndex)))
                            {
                                NeighbourCounter += 1;
                                NeighbourSum += P.Vertices[NeighbourIndex].Tog3Pt();
                                if (!ThisVertex.IsBoundary && HardCreases[ThisCreaseIndex]) PullCreases.Add(ThisCreaseIndex);
                            }
                        }
                    }
                    NewPosition = P.Vertices[v].Tog3Pt() * 0.75;
                    NewPosition += (NeighbourSum / NeighbourCounter) * 0.25;
                    if (PullCreases.Count>0)
                    {
                        Vector3d PulledPosition = new Vector3d();
                        foreach (int PullCrease in PullCreases)
                        {
                            var tempPt01=Creases[PullCrease].ClosestPoint(NewPosition, out int crtIndex, out double t);
                            //PulledPosition += Creases[PullCrease].PointAt(crtIndex, t) / PullCreases.Count;
                            PulledPosition += tempPt01 / PullCreases.Count;
                        }
                        NewPosition = PulledPosition;
                    }
                    Solved = true;
                }

                if (!Solved)// then it's a normal, even vertex
                {
                    if (Algorithm==0)
                    {
                        //Catmull-Clark
                        int[] InHEs = P.Vertices.GetIncomingHalfedges(v);

                        double Beta = 3.0 / (2 * InHEs.Length);
                        double Delta = 1.0 / (4 * InHEs.Length);
                        double BetaK = Beta / InHEs.Length;
                        double DeltaK = Delta / InHEs.Length;

                        NewPosition = P.Vertices[v].Tog3Pt() * (1 - Beta - Delta);

                        foreach (int InHE in InHEs)
                        {
                            NewPosition += P.Vertices[P.Halfedges[InHE].StartVertex].Tog3Pt() * BetaK;
                            NewPosition += P.Vertices[P.Halfedges[P.Halfedges[InHE].PrevHalfedge].StartVertex].Tog3Pt() * DeltaK;
                        }
                    }
                    else
                    {
                        //Loop
                        int[] Neighbours = P.Vertices.GetVertexNeighbours(v);

                        int Valence = Neighbours.Length;
                        double Beta = 0.625 - (Math.Pow((3 + 2 * Math.Cos((2 * Math.PI) / Valence)), 2) * 0.015625);
                        double Mult = Beta / Valence;

                        NewPosition = P.Vertices[v].Tog3Pt() * (1 - Beta);

                        foreach (int Neighbour in Neighbours)
                        {
                            NewPosition += P.Vertices[Neighbour].Tog3Pt() * Mult;
                        }
                    }
                }
                NP.Vertices.Add(NewPosition);
            }

            // cycle through each halfedge pair to set odd vertices
            for (int HE = 0; HE <P.Halfedges.Count; HE+=2)
            {
                int Pair = P.Halfedges.GetPairHalfedge(HE);
                int ThisOpp = P.Halfedges[HE].PrevHalfedge;
                int PairOpp = P.Halfedges[Pair].PrevHalfedge;

                int HeSV = P.Halfedges[HE].StartVertex;
                int PrSV = P.Halfedges[Pair].StartVertex;

                // split the halfedges in the topology lookup mesh
                TP.Halfedges.SplitEdge(HE);
                // ensure that each face has a starting halfedge on an even vertex
                if (P.Halfedges[Pair].AdjacentFace > -1) TP.Faces[P.Halfedges[Pair].AdjacentFace].FirstHalfedge = TP.Halfedges.Count - 1;

                VertexData ThisVertex = VerticesData[HeSV];
                VertexData PairVertex = VerticesData[PrSV];
                VertexData NewVertex = new VertexData(false, P.Halfedges.IsBoundary(HE));

                Vector3d NewPosition = new Vector3d();
                bool Solved = false;

                // check for boundary or crease condition and apply weights
                if (ThisVertex.NeighbourIndices.Contains(PrSV) && PairVertex.NeighbourIndices.Contains(HeSV))
                {
                    int ThisCrease = ThisVertex.CreaseIndices[ThisVertex.NeighbourIndices.IndexOf(PrSV)];
                    if (ThisCrease==PairVertex.CreaseIndices[PairVertex.NeighbourIndices.IndexOf(HeSV)])
                    {
                        NewPosition = P.Vertices[HeSV].Tog3Pt() * 0.5 +
                            P.Vertices[PrSV].Tog3Pt() * 0.5;
                        NewVertex.CreaseIndices.Add(ThisCrease);
                        NewVertex.CreaseIndices.Add(ThisCrease);
                        NewVertex.NeighbourIndices.Add(HeSV);
                        NewVertex.NeighbourIndices.Add(PrSV);
                        ThisVertex.NeighbourIndices[ThisVertex.NeighbourIndices.IndexOf(PrSV)] = NP.Vertices.Count;
                        PairVertex.NeighbourIndices[PairVertex.NeighbourIndices.IndexOf(HeSV)] = NP.Vertices.Count;

                        if (ThisCrease > -1) // check for hard creases in non-boundary conditions
                        {
                            if (HardCreases[ThisCrease])
                            {
                                var tempPt01=Creases[ThisCrease].ClosestPoint(NewPosition, out int CrvIndex, out double t);
                                //NewPosition = Creases[ThisCrease].PointAt(CrvIndex,t);
                                NewPosition = tempPt01;
                            }
                        }
                        Solved = true;
                    }
                }

                if (!Solved)
                {

                    if (Algorithm == 0)
                    {
                        // Catmull-Clark
                        NewPosition = P.Vertices[HeSV].Tog3Pt()* 0.375 +
                          P.Vertices[PrSV].Tog3Pt() * 0.375 +
                          P.Vertices[P.Halfedges[P.Halfedges.GetPairHalfedge(P.Halfedges[HE].NextHalfedge)].StartVertex].Tog3Pt() * 0.0625 +
                          P.Vertices[P.Halfedges[P.Halfedges[HE].PrevHalfedge].StartVertex].Tog3Pt() * 0.0625 +
                          P.Vertices[P.Halfedges[P.Halfedges.GetPairHalfedge(P.Halfedges[Pair].NextHalfedge)].StartVertex].Tog3Pt() * 0.0625 +
                          P.Vertices[P.Halfedges[P.Halfedges[Pair].PrevHalfedge].StartVertex].Tog3Pt() * 0.0625;
                    }
                    else
                    {
                        // Loop
                        NewPosition = P.Vertices[HeSV].Tog3Pt() * 0.375 +
                          P.Vertices[PrSV].Tog3Pt() * 0.375 +
                          P.Vertices[P.Halfedges[ThisOpp].StartVertex].Tog3Pt() * 0.125 +
                          P.Vertices[P.Halfedges[PairOpp].StartVertex].Tog3Pt() * 0.125;
                    }
                }

                VerticesData.Add(NewVertex);
                NP.Vertices.Add(NewPosition);
            }//end he cycle

            //add center point to each face for Catmull-Clark
            if (Algorithm == 0)
            {
                // add center point for each face and build subdivided faces in the new planktonmesh
                for (int f = 0; f < P.Faces.Count; f++)
                {
                    int CenterIdx = NP.Vertices.Count;
                    NP.Vertices.Add(P.Faces.GetFaceCenter(f).Tog3Pt());
                    VerticesData.Add(new VertexData(false, false));
                    int[] FaceVerts = TP.Faces.GetFaceVertices(f);
                    for (int nf = 0; nf < FaceVerts.Length-1; nf += 2)
                    {
                        int LastVert = nf - 1;
                        if (nf == 0) LastVert = FaceVerts.Length - 1;
                        NP.Faces.AddFace(FaceVerts[nf], FaceVerts[nf + 1], CenterIdx, FaceVerts[LastVert]);
                    }
                }
            }
            else
            {
                for (int f = 0; f < P.Faces.Count; f++)
                {
                    // add new triangulated faces
                    int[] FaceVerts = TP.Faces.GetFaceVertices(f);
                    NP.Faces.AddFace(FaceVerts[0], FaceVerts[1], FaceVerts[5]);
                    NP.Faces.AddFace(FaceVerts[2], FaceVerts[3], FaceVerts[1]);
                    NP.Faces.AddFace(FaceVerts[4], FaceVerts[5], FaceVerts[3]);
                    NP.Faces.AddFace(FaceVerts[1], FaceVerts[3], FaceVerts[5]);
                }
            }

            return NP;

        }

        private static List<DCurve3> ExtractEdge(DMesh3 meshIn)
        {
            var edges = meshIn.BoundaryEdgeIndices().ToList();
            var count = edges.Count;
            var crvList = new List<DCurve3>(count);

            for (int i = 0; i < count; i++)
            {
                var edge = meshIn.GetEdge(edges[i]);
                crvList.Add(new DCurve3(new Vector3d[] { meshIn.GetVertex(edge.a), meshIn.GetVertex(edge.b) }, false));
                
            }
            return crvList;
        }

        private static DCurve3[] ExtractEdge(Vector3d[] pts, double baseHeight, double height)
        {
            var count = pts.Length;
            var result = new DCurve3[count-1];
            for (int i = 0; i < count-1; i++)
            {
                var pt = pts[i];
                result[i]=new DCurve3(new Vector3d[] { new Vector3d(pt.x, pt.y, baseHeight), new Vector3d(pt.x, pt.y, baseHeight+height) },false);
            }
            return result;
        }
        private static Interval1d ReMap(Interval1d orginInter,Interval1d newInter)
        {
            return new Interval1d(orginInter.a / orginInter.b, 1);
        }
        #endregion


        #endregion

        #region 003_Intersection
        public static List<double> CalcRaysThroughTri(DMesh3 meshIn, NTS.Geometries.Point[] ptArray, double viewRange, Dictionary<int, double> areaDic, VisDataType visType, DateTime start, out Dictionary<int, int> MeshIntrCountDic)
        {
            DMesh3 mesh = new DMesh3(meshIn);
            var count = mesh.TriangleCount;
            //var viewPtList = NTSPtList2Vector3dList_3d(ptArray);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(mesh);
            spatial.Build();
            //ToolManagers.TimeCalculation(start, "空间树创建");

            Dictionary<int, int> meshIntrCountDic = new Dictionary<int, int>();// meshVertex Index, hit count
            Dictionary<int, double> viewPtIntrAreaDic = new Dictionary<int, double>();//viewPoint Index, hit mesh area

            NTS.Index.Quadtree.Quadtree<NTS.Geometries.Point> quadTree = new NTS.Index.Quadtree.Quadtree<NTS.Geometries.Point>();
            for (int i = 0; i < ptArray.Length; i++)
                quadTree.Insert(ptArray[i].EnvelopeInternal, ptArray[i]);
            //ToolManagers.TimeCalculation(start, "四叉树创建");

            for (int meshIndex = 0; meshIndex < count; meshIndex++)
            {
                var trisNormals = -mesh.GetTriNormal(meshIndex);
                var triArea = mesh.GetTriArea(meshIndex);
                //debugNormalList.Add(trisNormals);
                if (trisNormals.z == 1d || trisNormals.z == -1d)
                    continue;

                var centroid = mesh.GetTriCentroid(meshIndex);
                var vertexList = mesh.GetTriangle(meshIndex);
                int[] indexList = new int[3] { vertexList.a, vertexList.b, vertexList.c };

                //To Do 用NTS进行四叉树索引
                var centroid4Tree = centroid.toNTSPt();
                var mainCoor = new NTS.Geometries.Coordinate(centroid4Tree.X, centroid4Tree.Y);
                var tempEnv = Poly2DCreation.CreateEnvelopeFromPt(centroid4Tree, viewRange);
                var secPtListQuery = quadTree.Query(tempEnv);
                List<Vector3d> viewPtList = new List<Vector3d>();
                for (int j = 0; j < secPtListQuery.Count; j++)
                {
                    var secPt = secPtListQuery[j];
                    NTS.Geometries.Coordinate secCoor = new NTS.Geometries.Coordinate(secPt.X, secPt.Y);
                    double dis = mainCoor.Distance(secCoor);
                    //if (dis < viewRange)
                    viewPtList.Add(secPt.tog3Pt());
                }
                //ToolManagers.TimeCalculation(start, $"{meshIndex} 四叉树排除点");

                for (int viewPtIndex = 0; viewPtIndex < viewPtList.Count; viewPtIndex++)
                {
                    var direction = viewPtList[viewPtIndex]- centroid;

                    //判定方向，是否同向
                    var angle = (trisNormals).Dot(direction);

                    //判定距离，是否在视域内
                    var distance = centroid.Distance(viewPtList[viewPtIndex]);

                    //debugAngleList.Add(angle);
                    //debugDistanceList.Add(distance);

                    if (angle > 0 && distance < viewRange)
                    {
                        #region 计算被击中的次数
                        Ray3d ray = new Ray3d(viewPtList[viewPtIndex], -direction);
                        int hit_tid = spatial.FindNearestHitTriangle(ray);
                        if (hit_tid != DMesh3.InvalidID)
                        {
                            #region 计算射线距离
                            double hit_dist = -1d;
                            IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(mesh, hit_tid, ray);
                            //hit_dist = centroid.Distance(ray.PointAt(intr.RayParameter));
                            #endregion

                            //double intrDistance=ray.PointAt(rayT).Distance(viewPtList[viewPtIndex]);
                            if (Math.Abs(intr.RayParameter - distance)<0.0001)
                            {
                                if (viewPtIntrAreaDic.ContainsKey(viewPtIndex))
                                    viewPtIntrAreaDic[viewPtIndex] += triArea;
                                else
                                    viewPtIntrAreaDic.Add(viewPtIndex, triArea);

                                for (int vertexIndex = 0; vertexIndex < 3; vertexIndex++)
                                {
                                    if (meshIntrCountDic.ContainsKey(indexList[vertexIndex]))
                                    {
                                        meshIntrCountDic[indexList[vertexIndex]] += 1;
                                    }
                                    else
                                    {
                                        meshIntrCountDic.Add(indexList[vertexIndex], 1);
                                    }
                                }
                            }
                        }
                        #endregion

                    }
                }
                
            }

            var visRatio = new List<double>(areaDic.Count);
            switch (visType)
            {
                case VisDataType.TotalVisArea:
                    for (int i = 0; i < areaDic.Count; i++)
                    {
                        if (!viewPtIntrAreaDic.ContainsKey(i))
                            visRatio.Add(0d);
                        else
                            visRatio.Add(viewPtIntrAreaDic[i]);
                    }
                    break;
                case VisDataType.VisRatio:
                    for (int i = 0; i < areaDic.Count; i++)
                    {
                        if (!viewPtIntrAreaDic.ContainsKey(i))
                            visRatio.Add(0d);
                        else
                            visRatio.Add(viewPtIntrAreaDic[i] / areaDic[i]);
                    }
                    break;
                case VisDataType.normalizedVisRatio:
                    var total = viewPtIntrAreaDic.Values.ToList().Sum();
                    for (int i = 0; i < areaDic.Count; i++)
                    {
                        if (!viewPtIntrAreaDic.ContainsKey(i))
                            visRatio.Add(0d);
                        else
                            visRatio.Add(viewPtIntrAreaDic[i]/ total);
                    }
                    break;
            }
            //ToolManagers.TimeCalculation(start, "vis值计算");

            MeshIntrCountDic = meshIntrCountDic;
            return visRatio;
        }

        public static List<double> CalcRaysThroughTriParallel(DMesh3 meshIn, NTS.Geometries.Point[] ptArray, double viewRange, Dictionary<NTS.Geometries.Point, double> ptAreaDic, VisDataType visType, DateTime start, out ConcurrentDictionary<int, int> MeshIntrCountDic)
        {
            DMesh3 mesh = new DMesh3(meshIn);
            var count = mesh.TriangleCount;
            //var viewPtList = NTSPtList2Vector3dList_3d(ptArray);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(mesh);
            spatial.Build();
            ToolManagers.TimeCalculation(start, "空间树创建");

            ConcurrentDictionary<int, int> meshIntrCountDic = new ConcurrentDictionary<int, int>();// meshVertex Index, hit count
            ConcurrentDictionary<int, double> viewPtIntrAreaDic = new ConcurrentDictionary<int, double>();//viewPoint Index, hit mesh area
            ConcurrentDictionary<NTS.Geometries.Point, int> viewPtIndexDic = new ConcurrentDictionary<NTS.Geometries.Point, int>();

            for (int i = 0; i < ptArray.Length; i++)
            {
                var tempPt = ptArray[i];
                if (viewPtIndexDic.ContainsKey(tempPt))
                    viewPtIndexDic[tempPt] = i;
                else
                    viewPtIndexDic.TryAdd(tempPt, i);
            }

            NTS.Index.Quadtree.Quadtree<NTS.Geometries.Point> quadTree = new NTS.Index.Quadtree.Quadtree<NTS.Geometries.Point>();
            for (int i = 0; i < ptArray.Length; i++)
                quadTree.Insert(ptArray[i].EnvelopeInternal, ptArray[i]);
            ToolManagers.TimeCalculation(start, "四叉树创建");


            System.Threading.Tasks.Parallel.For(0, count, meshIndex =>
            {
                var trisNormals = - mesh.GetTriNormal(meshIndex);
                var triArea = mesh.GetTriArea(meshIndex);

                var centroid = mesh.GetTriCentroid(meshIndex);
                var vertexList = mesh.GetTriangle(meshIndex);
                int[] indexList = new int[3] { vertexList.a, vertexList.b, vertexList.c };

                //To Do 用NTS进行四叉树索引
                var centroid4Tree = centroid.toNTSPt();
                //var mainCoor = new NTS.Geometries.Coordinate(centroid4Tree.X, centroid4Tree.Y);
                var tempEnv = Poly2DCreation.CreateEnvelopeFromPt(centroid4Tree, viewRange);
                var secPtListQuery = quadTree.Query(tempEnv);
                var viewPtList =new Vector3d[secPtListQuery.Count] ;
                for (int j = 0; j < secPtListQuery.Count; j++)
                {
                    var secPt = secPtListQuery[j];
                    //NTS.Geometries.Coordinate secCoor = new NTS.Geometries.Coordinate(secPt.X, secPt.Y);
                    //double dis = mainCoor.Distance(secCoor);
                    //if (dis < viewRange)
                    viewPtList[j]=(secPt.tog3Pt());
                }
                //ToolManagers.TimeCalculation(start, $"{meshIndex} 四叉树排除点");

                for (int viewPtIndex = 0; viewPtIndex < viewPtList.Length; viewPtIndex++)
                {
                    var direction = viewPtList[viewPtIndex] - centroid;

                    //判定方向，是否同向
                    var angle = (trisNormals).Dot(direction);

                    //判定距离，是否在视域内
                    var distance = centroid.Distance(viewPtList[viewPtIndex]);

                    if (angle > 0 && distance < viewRange)
                    {
                        #region 计算被击中的次数
                        Ray3d ray = new Ray3d(viewPtList[viewPtIndex], -direction);
                        int hit_tid = spatial.FindNearestHitTriangle(ray);
                        if (hit_tid != DMesh3.InvalidID)
                        {
                            #region 计算射线距离
                            double hit_dist = -1d;
                            IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(mesh, hit_tid, ray);
                            //hit_dist = centroid.Distance(ray.PointAt(intr.RayParameter));
                            #endregion

                            //double intrDistance=ray.PointAt(rayT).Distance(viewPtList[viewPtIndex]);
                            if (Math.Abs(intr.RayParameter - distance) < 0.0001)
                            {
                                var tempPt = viewPtList[viewPtIndex].toNTSPt();
                                var tempViewPtIndex = viewPtIndexDic[tempPt];

                                if (viewPtIntrAreaDic.ContainsKey(tempViewPtIndex))
                                {
                                    var tempTriArea = viewPtIntrAreaDic[tempViewPtIndex];
                                    viewPtIntrAreaDic.TryUpdate(tempViewPtIndex, tempTriArea + triArea, tempTriArea);
                                    //viewPtIntrAreaDic[viewPtIndex] += triArea;
                                }
                                else
                                    viewPtIntrAreaDic.TryAdd(tempViewPtIndex, triArea);

                                for (int vertexIndex = 0; vertexIndex < 3; vertexIndex++)
                                {
                                    if (meshIntrCountDic.ContainsKey(indexList[vertexIndex]))
                                    {
                                        var tempCount = meshIntrCountDic[indexList[vertexIndex]];
                                        meshIntrCountDic.TryUpdate(indexList[vertexIndex], tempCount + 1, tempCount);
                                        //meshIntrCountDic[indexList[vertexIndex]] += 1;
                                    }
                                    else
                                    {
                                        meshIntrCountDic.TryAdd(indexList[vertexIndex], 1);
                                    }
                                }
                            }
                        }
                        #endregion

                    }
                }
                //ToolManagers.TimeCalculation(start, $"{meshIndex} 判断相切");
            });

            ToolManagers.TimeCalculation(start, "相切计算");

            var visRatio = new List<double>(ptArray.Length);
            switch (visType)
            {
                case VisDataType.TotalVisArea:
                    for (int i = 0; i < ptArray.Length; i++)
                    {
                        if (!viewPtIntrAreaDic.ContainsKey(i))
                            visRatio.Add(0d);
                        else
                            visRatio.Add(viewPtIntrAreaDic[i]);
                    }
                    break;
                case VisDataType.VisRatio:

                    Dictionary<int, double> areaDic = Poly2DCreation.ContainsAreaInPts(ptArray, ptAreaDic, viewRange);
                    for (int i = 0; i < ptArray.Length; i++)
                    {
                        if (!viewPtIntrAreaDic.ContainsKey(i))
                            visRatio.Add(0d);
                        else
                            visRatio.Add(viewPtIntrAreaDic[i] / areaDic[i]);
                    }
                    break;
                case VisDataType.normalizedVisRatio:
                    var total = viewPtIntrAreaDic.Values.ToList().Sum();
                    for (int i = 0; i < ptArray.Length; i++)
                    {
                        if (!viewPtIntrAreaDic.ContainsKey(i))
                            visRatio.Add(0d);
                        else
                            visRatio.Add(viewPtIntrAreaDic[i] / total);
                    }
                    break;
            }

            MeshIntrCountDic = meshIntrCountDic;
            return visRatio;
        }

        public static List<double> CalcRaysThroughTri(DMesh3 meshIn, NTS.Geometries.Point[] ptArray, double viewRange, Dictionary<NTS.Geometries.Point, double> ptAreaDic, VisDataType visType, out Dictionary<int, int> MeshIntrCountDic)
        {
            DMesh3 mesh = new DMesh3(meshIn);
            var count = mesh.TriangleCount;
            //var viewPtList = NTSPtList2Vector3dList_3d(ptArray);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(mesh);
            spatial.Build();

            Dictionary<int, int> meshIntrCountDic = new Dictionary<int, int>();// meshVertex Index, hit count
            Dictionary<int, double> viewPtIntrAreaDic = new Dictionary<int, double>();//viewPoint Index, hit mesh area

            //Dictionary<int, List<int>> debug_ptMeshIndex = new Dictionary<int, List<int>>();

            Dictionary<NTS.Geometries.Point, int> viewPtIndexDic = new Dictionary<NTS.Geometries.Point, int>();
            for (int i = 0; i < ptArray.Length; i++)
            {
                var tempPt = ptArray[i];
                if (viewPtIndexDic.ContainsKey(tempPt))
                    viewPtIndexDic[tempPt] = i;
                else
                    viewPtIndexDic.Add(tempPt, i);
            }
                

            NTS.Index.Quadtree.Quadtree<NTS.Geometries.Coordinate> quadTree = new NTS.Index.Quadtree.Quadtree<NTS.Geometries.Coordinate>();
            for (int i = 0; i < ptArray.Length; i++)
                quadTree.Insert(ptArray[i].EnvelopeInternal, ptArray[i].Coordinate);

            for (int meshIndex = 0; meshIndex < count; meshIndex++)
            {
                var trisNormals = -mesh.GetTriNormal(meshIndex);
                var triArea = mesh.GetTriArea(meshIndex);
                //debugNormalList.Add(trisNormals);
                if (trisNormals.z == 1d || trisNormals.z == -1d)
                    continue;

                var centroid = mesh.GetTriCentroid(meshIndex);
                var vertexList = mesh.GetTriangle(meshIndex);
                int[] indexList = new int[3] { vertexList.a, vertexList.b, vertexList.c };

                var centroid4Tree = centroid.toNTSPt();
                //var mainCoor = new NTS.Geometries.Coordinate(centroid4Tree.X, centroid4Tree.Y);
                var tempEnv = Poly2DCreation.CreateEnvelopeFromPt(centroid4Tree, viewRange);
                var secPtListQuery = quadTree.Query(tempEnv);
                
                List<Vector3d> viewPtList = new List<Vector3d>();

                for (int j = 0; j < secPtListQuery.Count; j++)
                {
                    var secPt = secPtListQuery[j];
                    //NTS.Geometries.Coordinate secCoor = new NTS.Geometries.Coordinate(secPt.X, secPt.Y);
                    //double dis = mainCoor.Distance(secCoor);
                    //if (dis < viewRange)
                    viewPtList.Add(new NTS.Geometries.Point(secPt).tog3Pt());
                }

                for (int viewPtIndex = 0; viewPtIndex < viewPtList.Count; viewPtIndex++)
                {
                    var direction = viewPtList[viewPtIndex] - centroid;

                    //判定方向，是否同向
                    var angle = (trisNormals).Dot(direction);

                    //判定距离，是否在视域内
                    var distance = centroid.Distance(viewPtList[viewPtIndex]);

                    //debugAngleList.Add(angle);
                    //debugDistanceList.Add(distance);

                    if (angle > 0 && distance < viewRange)
                    {
                        #region 计算被击中的次数
                        Ray3d ray = new Ray3d(viewPtList[viewPtIndex], -direction);
                        int hit_tid = spatial.FindNearestHitTriangle(ray);
                        if (hit_tid != DMesh3.InvalidID)
                        {
                            #region 计算射线距离
                            double hit_dist = -1d;
                            IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(mesh, hit_tid, ray);
                            //hit_dist = centroid.Distance(ray.PointAt(intr.RayParameter));
                            #endregion

                            //double intrDistance=ray.PointAt(rayT).Distance(viewPtList[viewPtIndex]);
                            if (Math.Abs(intr.RayParameter - distance) < 0.0001)
                            {
                                var tempPt = viewPtList[viewPtIndex].toNTSPt();
                                var tempViewPtIndex = viewPtIndexDic[tempPt];

                                //if (debug_ptMeshIndex.ContainsKey(tempViewPtIndex))
                                //    debug_ptMeshIndex[tempViewPtIndex].Add(meshIndex);
                                //else
                                //    debug_ptMeshIndex.Add(tempViewPtIndex, new List<int>() { meshIndex});

                                if (viewPtIntrAreaDic.ContainsKey(tempViewPtIndex))
                                    viewPtIntrAreaDic[tempViewPtIndex] += triArea;
                                else
                                    viewPtIntrAreaDic.Add(tempViewPtIndex, triArea);


                                for (int vertexIndex = 0; vertexIndex < 3; vertexIndex++)
                                {
                                    if (meshIntrCountDic.ContainsKey(indexList[vertexIndex]))
                                    {
                                        meshIntrCountDic[indexList[vertexIndex]] += 1;
                                    }
                                    else
                                    {
                                        meshIntrCountDic.Add(indexList[vertexIndex], 1);
                                    }
                                }
                            }
                        }
                        #endregion

                    }
                }
            }

            var visRatio = new List<double>(ptArray.Length);
            switch (visType)
            {
                case VisDataType.TotalVisArea:
                    for (int i = 0; i < ptArray.Length; i++)
                    {
                        if (!viewPtIntrAreaDic.ContainsKey(i))
                            visRatio.Add(0d);
                        else
                            visRatio.Add(viewPtIntrAreaDic[i]);
                    }
                    break;
                case VisDataType.VisRatio:

                    Dictionary<int, double> areaDic= Poly2DCreation.ContainsAreaInPts(ptArray, ptAreaDic, viewRange);
                    for (int i = 0; i < ptArray.Length; i++)
                    {
                        if (!viewPtIntrAreaDic.ContainsKey(i))
                            visRatio.Add(0d);
                        else
                            visRatio.Add(viewPtIntrAreaDic[i] / areaDic[i]);
                    }
                    break;
                case VisDataType.normalizedVisRatio:
                    var total = viewPtIntrAreaDic.Values.ToList().Sum();
                    for (int i = 0; i < ptArray.Length; i++)
                    {
                        if (!viewPtIntrAreaDic.ContainsKey(i))
                            visRatio.Add(0d);
                        else
                            visRatio.Add(viewPtIntrAreaDic[i] / total);
                    }
                    break;
            }

            MeshIntrCountDic = meshIntrCountDic;
            return visRatio;
        }

        public static Dictionary<int, int> CalcRays(DMesh3 mesh, Vector3d origin, int segmentHeight = 10, int segment = 10, double angle = 360, double radius = 100, double angleHeight = 90)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(meshIn);
            spatial.Build();

            var direction = CreateSphereDirection(origin, segmentHeight, segment, angle, radius, angleHeight);
            //number of hitted vertex
            Dictionary<int, int> hitIndexDic = new Dictionary<int, int>();
            for (int i = 0; i < direction.Length; i++)
            {
                Ray3d ray = new Ray3d(origin, direction[i]);

                #region 计算被击中的次数
                int hit_tid = spatial.FindNearestHitTriangle(ray);
                if (hit_tid != DMesh3.InvalidID)
                {
                    #region 计算射线距离
                    double hit_dist = -1d;
                    IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(mesh, hit_tid, ray);
                    hit_dist = origin.Distance(ray.PointAt(intr.RayParameter));
                    #endregion

                    #region 判定是否在距离内，如果是，提取三角形的顶点序号
                    if (hit_dist <= radius)
                    {
                        var tempTri = meshIn.GetTriangle(hit_tid);
                        for (int eachVertex = 0; eachVertex < tempTri.array.Length; eachVertex++)
                        {
                            var hit_vid = tempTri[eachVertex];
                            if (hitIndexDic.ContainsKey(hit_vid))
                            {
                                var temp_amount = hitIndexDic[hit_vid];
                                hitIndexDic[hit_vid] = temp_amount + 1;
                            }
                            else
                            {
                                hitIndexDic.Add(hit_vid, 1);
                            }
                        }
                    }
                    #endregion
                }
                #endregion
            }
            return hitIndexDic;
            //return hitTrianglesDic;
        }

        public static Dictionary<int, int> CalcRays(DMesh3 mesh, IEnumerable<Vector3d> originList, int segmentHeight = 10, int segment = 10, double angle = 360, double radius = 100, double angleHeight = 90)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(meshIn);
            spatial.Build();
            Dictionary<int, int> hitIndexDic = new Dictionary<int, int>();

            for (int ptIndex = 0; ptIndex < originList.Count(); ptIndex++)
            {
                var origin = originList.ElementAt(ptIndex);
                var direction = CreateSphereDirection(origin, segmentHeight, segment, angle, radius, angleHeight);
                //number of hitted vertex
                for (int i = 0; i < direction.Length; i++)
                {
                    Ray3d ray = new Ray3d(origin, direction[i]);

                    #region 计算被击中的次数
                    int hit_tid = spatial.FindNearestHitTriangle(ray);
                    if (hit_tid != DMesh3.InvalidID)
                    {
                        #region 计算射线距离
                        double hit_dist = -1d;
                        IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(mesh, hit_tid, ray);
                        hit_dist = origin.Distance(ray.PointAt(intr.RayParameter));
                        #endregion

                        #region 判定是否在距离内，如果是，存入三角形序号
                        if (hit_dist <= radius)
                        {
                            var tempTri = meshIn.GetTriangle(hit_tid);
                            for (int eachVertex = 0; eachVertex < tempTri.array.Length; eachVertex++)
                            {
                                var hit_vid = tempTri[eachVertex];
                                if (hitIndexDic.ContainsKey(hit_vid))
                                {
                                    var temp_amount = hitIndexDic[hit_vid];
                                    hitIndexDic[hit_vid] = temp_amount + 1;
                                }
                                else
                                {
                                    hitIndexDic.Add(hit_vid, 1);
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }
            return hitIndexDic;
            //return hitTrianglesDic;
        }

        public static double[] CalcRaysThroughPt(DMesh3 mesh, IEnumerable<Vector3d> originList, int segmentHeight = 10, int segment = 10, double angle = 360, double radius = 100, double angleHeight = 90)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(meshIn);
            spatial.Build();
            double[] hitTriArea = new double[originList.Count()];

            for (int ptIndex = 0; ptIndex < originList.Count(); ptIndex++)
            {
                var origin = originList.ElementAt(ptIndex);
                var direction = CreateSphereDirection(origin, segmentHeight, segment, angle, radius, angleHeight);
                var rayArea = 0d;
                //number of hitted vertex
                for (int i = 0; i < direction.Length; i++)
                {
                    Ray3d ray = new Ray3d(origin, direction[i]);

                    #region 计算被击中的次数
                    int hit_tid = spatial.FindNearestHitTriangle(ray);
                    if (hit_tid != DMesh3.InvalidID)
                    {
                        #region 计算射线距离
                        double hit_dist = -1d;
                        IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(mesh, hit_tid, ray);
                        hit_dist = origin.Distance(ray.PointAt(intr.RayParameter));
                        #endregion

                        #region 判定是否在距离内，如果是，加上先前面积
                        if (hit_dist <= radius)
                        {
                            var triArea = meshIn.GetTriArea(hit_tid);
                            rayArea += triArea;
                        }
                        #endregion
                    }
                    #endregion
                }

                hitTriArea[ptIndex] = rayArea;
            }
            return hitTriArea;
            //return hitTrianglesDic;
        }
        /// <summary>
        /// based on origin, create a sphere
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="segment">subdivision count</param>
        /// <param name="angle">horizontal visible angle</param>
        /// <param name="radius">how far could we see</param>
        /// <param name="angleHeight">vertical visible angle, <=90</param>
        public static Vector3d[] CreateSphereDirection(Vector3d origin, int segmentHeight, int segment, double angle, double radius, double angleHeight)
        {
            if (angleHeight > 90)
                angleHeight = 90;
            if (angle > 360)
                angle = 360;
            double _angleHeight = Math.PI * angleHeight / 180 / segmentHeight;
            double _angle = Math.PI * angle / 180 / segment;

            Vector3d[] vertices = new Vector3d[(segmentHeight) * (segment)];
            int index = 0;

            for (int y = 0; y < segmentHeight; y++)
            {
                for (int x = 0; x < segment; x++)
                {
                    double _z = Math.Sin(y * _angleHeight) * radius;
                    double _x = Math.Cos(y * _angleHeight) * Math.Cos(x * _angle) * radius;
                    double _y = Math.Cos(y * _angleHeight) * Math.Sin(x * _angle) * radius;
                    vertices[index] = new Vector3d(new Vector3d(origin.x + _x, origin.y + _y, origin.z + _z) - origin);
                    index++;
                }
            }

            return vertices;
        }

        public static DMesh3 ApplyColorsBasedOnRays(DMesh3 mesh, Dictionary<int, int> hitTrianglesDic, Colorf originColor, Colorf DestnationColor)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            try
            {
                var maxNumber = hitTrianglesDic.Values.Max();

                int i = 0;

                foreach (var item in hitTrianglesDic)
                {
                    //var scalarValue = item.Value / (float)maxNumber;
                    var scalarValue = (float)Math.Log(item.Value, maxNumber);
                    var tempColor = Colorf.Lerp(originColor, DestnationColor, scalarValue);
                    meshIn.SetVertexColor(item.Key, tempColor);
                    i++;
                }

            }
            catch
            {
                throw new MyException("there is no intersection between brep and view point");
            }

            return meshIn;
        }

        public static DMesh3 ApplyColorsBasedOnRays(DMesh3 mesh, ConcurrentDictionary<int, int> hitTrianglesDic, Colorf originColor, Colorf DestnationColor)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            try
            {
                var maxNumber = hitTrianglesDic.Values.Max();

                int i = 0;

                foreach (var item in hitTrianglesDic)
                {
                    //var scalarValue = item.Value / (float)maxNumber;
                    var scalarValue = (float)Math.Log(item.Value, maxNumber);
                    var tempColor = Colorf.Lerp(originColor, DestnationColor, scalarValue);
                    meshIn.SetVertexColor(item.Key, tempColor);
                    i++;
                }

            }
            catch
            {
                throw new MyException("there is no intersection between brep and view point");
            }

            return meshIn;
        }

        public static double[] CalcVisibilityPercent(double[] visibleArea, double[] wholeArea)
        {
            double[] result = new double[visibleArea.Length];
            for (int i = 0; i < visibleArea.Length; i++)
            {
                result[i] = visibleArea[i] / wholeArea[i];
            }
            return result;
        }
        #endregion

        #region 004_Generating polyline
        public static Polygon2d[] CreatePolygon(string jsonFilePath)
        {
            var jsonData = ReadJsonData2D(jsonFilePath);
            var polygonList = new Polygon2d[jsonData.Length];
            for (int i = 0; i < jsonData.Length; i++)
            {
                Polygon2d polygon = new Polygon2d(jsonData[i]);
                polygonList[i] = polygon;
            }
            return polygonList;
        }

        public static Circle2d[] CreateCircle(Vector2d[] origin, double radius)
        {
            var circleList = new Circle2d[origin.Length];
            for (int i = 0; i < origin.Length; i++)
            {
                Circle2d circle = new Circle2d(origin[i], radius);
                circleList[i] = circle;
            }
            return circleList;

        }

        public static Vector2d[] ConvertV3toV2(Vector3d[] origin)
        {
            var result = new Vector2d[origin.Length];
            for (int i = 0; i < origin.Length; i++)
            {
                result[i] = new Vector2d(origin[i].x, origin[i].y);
            }
            return result;
        }

        public static Vector3d[] ConvertV2toV3(Vector2d[] origin, double[] zValue)
        {
            var result = new Vector3d[origin.Length];
            for (int i = 0; i < origin.Length; i++)
            {
                result[i] = new Vector3d(origin[i].x, origin[i].y, zValue[i]);
            }
            return result;
        }
        #endregion

        #region 005_convert
        #region NTS
        public static List<Vector3d> NTSPtList2Vector3dList_3d(IEnumerable<NTS.Geometries.Point> ptList)
        {
            List<Vector3d> vectorList = new List<Vector3d>(ptList.Count());
            for (int i = 0; i < ptList.Count(); i++)
                vectorList.Add(NTSPt2Vector3d(ptList.ElementAt(i)));
            return vectorList;
        }

        public static List<Vector3d> NTSPtList2Vector3dList_2d(IEnumerable<NTS.Geometries.Point> ptList)
        {
            List<Vector3d> vectorList = new List<Vector3d>(ptList.Count());
            for (int i = 0; i < ptList.Count(); i++)
                vectorList.Add(NTSPt2Vector3d(ptList.ElementAt(i), 0));
            return vectorList;
        }

        public static Vector3d NTSPt2Vector3d(NTS.Geometries.Point NTSPt, double height = 0)
        {
            return new Vector3d(NTSPt.X, NTSPt.Y, height);
        }

        public static Vector3d NTSPt2Vector3d(NTS.Geometries.Point NTSPt)
        {
            return new Vector3d(NTSPt.X, NTSPt.Y, NTSPt.Z);
        }

        #endregion

        #region RhinoMesh
        public static Rh.Mesh ConvertFromDMesh3(DMesh3 meshInput)
        {
            Rh.Mesh meshOutput = new Rh.Mesh();

            var tempMeshInputVertices = meshInput.Vertices().ToArray();
            var rhVerticesList = ConvertFromDMeshVector(tempMeshInputVertices);
            var rhFacesList = ConvertFromDMeshTri(meshInput.Triangles().ToArray());

            meshOutput.Vertices.AddVertices(rhVerticesList);
            meshOutput.Faces.AddFaces(rhFacesList);

            for (int i = 0; i < tempMeshInputVertices.Length; i++)
            {
                meshOutput.VertexColors.Add(ConvertFromDMeshColor(meshInput.GetVertexColor(i)));
            }

            meshOutput.Faces.ConvertTrianglesToQuads(0.034907, 0.875);
            return meshOutput;

        }

        public static Rh.Mesh ConvertFromDMesh3NoColor(DMesh3 meshInput)
        {
            Rh.Mesh meshOutput = new Rh.Mesh();

            var tempMeshInputVertices = meshInput.Vertices().ToArray();
            var rhVerticesList = ConvertFromDMeshVector(tempMeshInputVertices);
            var rhFacesList = ConvertFromDMeshTri(meshInput.Triangles().ToArray());

            meshOutput.Vertices.AddVertices(rhVerticesList);
            meshOutput.Faces.AddFaces(rhFacesList);

            return meshOutput;
        }

        private static Rh.Point3d[] ConvertFromDMeshVector(Vector3d[] meshVertices)
        {
            Rh.Point3d[] ptResult = new Rh.Point3d[meshVertices.Length];
            for (int i = 0; i < meshVertices.Length; i++)
            {
                ptResult[i] = new Rh.Point3d(meshVertices[i].x, meshVertices[i].y, meshVertices[i].z);
            }
            return ptResult;
        }

        private static Rh.MeshFace[] ConvertFromDMeshTri(Index3i[] meshTri)
        {
            Rh.MeshFace[] meshResult = new Rh.MeshFace[meshTri.Length];
            for (int i = 0; i < meshTri.Length; i++)
            {
                meshResult[i] = new Rh.MeshFace(meshTri[i].a, meshTri[i].b, meshTri[i].c);
            }
            return meshResult;
        }

        public static Color ConvertFromDMeshColor(Vector3f vertexColor)
        {
            var tempColor = ConvertColorValueBasedFloat(vertexColor.x, vertexColor.y, vertexColor.z);
            return Color.FromArgb(tempColor[0], tempColor[1], tempColor[2]);
        }

        public static Color ConvertFromDMeshColor(Colorf colorf)
        {
            var tempColor = ConvertColorValueBasedFloat(colorf.r, colorf.g, colorf.b);
            return Color.FromArgb(tempColor[0], tempColor[1], tempColor[2]);
        }
        private static int[] hsb2rgb(float h, float s, float v)
        {
            float r = 0, g = 0, b = 0;
            int i = (int)((h / 60) % 6);
            float f = (h / 60) - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);
            switch (i)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
                default:
                    break;
            }
            return new int[] { (int) (r * 255.0), (int) (g * 255.0),
            (int) (b * 255.0) };
        }

        private static int[] ConvertColorValueBasedFloat(float r, float g, float b)
        {
            int[] colorResult = new int[3];
            colorResult[0] = Math.Max(0, Math.Min(255, (int)Math.Floor(r * 256.0)));
            colorResult[1] = Math.Max(0, Math.Min(255, (int)Math.Floor(g * 256.0)));
            colorResult[2] = Math.Max(0, Math.Min(255, (int)Math.Floor(b * 256.0)));
            return colorResult;
        }
        #endregion

        #region PlanktonMesh


        public static NTS.Geometries.Point PMeshVertex2NTSPt(PlanktonVertex ptIn)
        {
            return new NTS.Geometries.Point(ptIn.X, ptIn.Y, ptIn.Z);
        }
        #endregion
        #endregion

        class MyException : Exception
        {
            public MyException(string message) : base(message)
            {
            }
        }
    }

    public class MeshInfoCollection
    {
        public int Id { get; set; }
        public NTS.Geometries.Point CentPt { get; set; }
        public double Area { get; set; }
    }

    public class VertexData
    {
        public bool IsAnchor;
        public bool IsBoundary;
        public List<int> CreaseIndices = new List<int>();
        public List<int> NeighbourIndices = new List<int>();

        public VertexData(bool SetAnchor, bool SetBoundary)
        {
            IsAnchor = SetAnchor;
            IsBoundary = SetBoundary;
        }
    }

    public class ViewPt
    {
        private double _wholeArea { get; set; }
        private double _viewArea { get; set; }
        
        private int _index;
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }
        public NTS.Geometries.Point _pt{ get; set; }

        private double _viewRatio;
        public double ViewRatio
        {
            get { 
                return _viewRatio; 
            }
            set {
                if (_wholeArea != 0)
                    _viewRatio = _viewArea / _wholeArea;
                else
                    _viewRatio = 0;
            }
        }

        public ViewPt(NTS.Geometries.Point point, int index, double viewArea, double wholeArea)
        {
            _pt = point;
            _index = index;
            _wholeArea = wholeArea;
            _viewArea = viewArea;
        }

        public ViewPt(NTS.Geometries.Point point, int index, double wholeArea)
        {
            _pt = point;
            _index = index;
            _wholeArea = wholeArea;
        }

    }

    internal class PointWithIndex
    {

    }
}