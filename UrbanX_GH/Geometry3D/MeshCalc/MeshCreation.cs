using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using g3;
using gs;
using System.IO;
using NTS = NetTopologySuite;
using Rh=Rhino.Geometry;
using System.Drawing;
using System.Collections.Concurrent;

namespace UrbanX_GH.Application.Geometry
{
    public enum BoundaryModes
    {
        FreeBoundaries,
        FixedBoundaries,
        ConstrainedBoundaries
    }
    public class MeshCreation
    {
        private const double tolerance= 0.0001;

        #region 000_Basic Function
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
        public static void InitiateColor(DMesh3 mesh, Colorf color)
        {
            mesh.EnableVertexColors(new Colorf(color));
        }

        public static DMesh3 ApplyColor(DMesh3 mesh, Colorf originColor, Colorf DestnationColor)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            float meshCount = meshIn.VertexCount;
            
            for (int i = 0; i < meshCount; i++)
            {
                var temp_color = Colorf.Lerp(originColor, DestnationColor, i/meshCount);
                meshIn.SetVertexColor(i, temp_color);
            }
            return meshIn;
        }

        public static DMesh3 ApplyColor(DMesh3 mesh, Colorf originColor, Colorf DestnationColor, float meshCount, Func<float,float>singleCount)
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
                
            DMesh3 mesh = DMesh3Builder.Build(vertices, triangles,normals);
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
                var meshSrf = BoundarySrfFromPts(OriginalData[i],out NTS.Geometries.Point centerPt);
                var meshExtruded = ExtrudeMeshFromMesh(meshSrf, height[i]);
                MeshEditor.Append(meshCollection, meshExtruded);
                var triAreaList = new List<double>(meshExtruded.TriangleCount);
                for (int j = 0; j < meshExtruded.TriangleCount; j++)
                {
                    triAreaList.Add(meshExtruded.GetTriArea(j));
                }
                var tempArea=triAreaList.Sum();

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
                var meshSrf = BoundarySrfFromPts(OriginalData[i], out NTS.Geometries.Point centerPt,out double meshArea);
                var meshExtruded = ExtrudeMeshFromMesh(meshSrf, height[i]);

                MeshEditor.Append(meshCollection, meshExtruded);
                var triAreaList = new List<double>(meshExtruded.TriangleCount);
                for (int j = 0; j < meshExtruded.TriangleCount; j++)
                {
                    triAreaList.Add(meshExtruded.GetTriArea(j));
                }
                var tempArea = triAreaList.Sum()-meshArea*2;

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
        public static DMesh3 ExtrudeMeshFromPt(Vector3d[][] OriginalData, double height=10d)
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
            centerPt=meshResult.CachedBounds.Center;
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
            MeshBoundaryLoops loops = new MeshBoundaryLoops(mesh);
            EdgeLoop eLoop = new EdgeLoop(mesh);
            eLoop.Edges = loops[0].Edges;
            eLoop.Vertices = loops[0].Vertices;
            new MeshExtrudeLoop(meshResult, eLoop)
            {
                PositionF = (v, n, vid) => v + height * Vector3d.AxisZ
            }.Extrude();

            //MeshLoopClosure meshClose = new MeshLoopClosure(mesh, eLoop);
            //meshClose.Close_Flat();
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
            new MeshExtrudeFaces(meshResult, triangles,true)
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
        public static DMesh3 SimpleRemesher(DMesh3 mesh,  double targetEdgeLength=1d, double smoothSpeedT=0.5d,bool reprojectToInput=false, bool preserve_creases=true, BoundaryModes boundaryMode=BoundaryModes.FixedBoundaries)
        {
            DMesh3 meshIn= new DMesh3(mesh);

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
                if (boundaryMode==BoundaryModes.FixedBoundaries)
                    MeshConstraintUtil.FixEdges(remesh.Constraints, meshIn, topo.BoundaryEdges);
            }else if (meshIn.CachedIsClosed == false)
            {
                if (remesh.Constraints == null)
                    remesh.SetExternalConstraints(new MeshConstraints());

                if (boundaryMode == BoundaryModes.FreeBoundaries)
                    MeshConstraintUtil.PreserveBoundaryLoops(remesh.Constraints, meshIn);
                else if(boundaryMode == BoundaryModes.FixedBoundaries)
                    MeshConstraintUtil.FixAllBoundaryEdges(remesh.Constraints, meshIn);
                else if(boundaryMode == BoundaryModes.ConstrainedBoundaries)
                    MeshConstraintUtil.FixAllBoundaryEdges_AllowSplit(remesh.Constraints, meshIn, 0);
            }

            remesh.FastestRemesh(25, true);

            // free boundary remesh can leave sliver triangles around the border. clean that up.
            if (meshIn.CachedIsClosed==false && boundaryMode==BoundaryModes.FreeBoundaries)
            {
                MeshEditor.RemoveFinTriangles(meshIn, (_mesh, tid) =>
                 {
                     Index3i tv = _mesh.GetTriangle(tid);
                     return MathUtil.AspectRatio(mesh.GetVertex(tv.a), mesh.GetVertex(tv.b), mesh.GetVertex(tv.c)) > 2;
                 });
            }

            DMesh3 meshOut = new DMesh3(meshIn,true);
            return meshOut;
        }
        #endregion

        #region 003_Intersection
        public static Dictionary<int, int> CalcRays(DMesh3 mesh, Vector3d origin, int segmentHeight = 10, int segment = 10, double angle = 360, double radius = 100, double angleHeight = 90)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(meshIn);
            spatial.Build();

            var direction = CreateSphereDirection(origin, segmentHeight,segment, angle,radius,angleHeight);
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
            }
            return hitIndexDic;
            //return hitTrianglesDic;
        }

        public static double[] CalcRaysGetArea(DMesh3 mesh, IEnumerable<Vector3d> originList, out Dictionary<int, int> CalcRaysDic, int segmentHeight = 10, int segment = 10, double angle = 360, double radius = 100, double angleHeight = 90)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(meshIn);
            spatial.Build();
            Dictionary<int, int> hitIndexDic = new Dictionary<int, int>();
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
                            var triArea=meshIn.GetTriArea(hit_tid);
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

                            rayArea += triArea;
                        }
                        #endregion
                    }
                    #endregion
                }

                hitTriArea[ptIndex] = rayArea;
            }

            CalcRaysDic = hitIndexDic;

            return hitTriArea;
            //return hitTrianglesDic;
        }


        public static DMeshAABBTree3 InitialMeshTree(DMesh3 mesh)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(meshIn);
            spatial.Build();
            return spatial;
        }
        public static double CalcRaysGetAreaParallel(DMesh3 meshIn, DMeshAABBTree3 spatial, Vector3d origin, ConcurrentDictionary<int, int> hitIndexDic, int segmentHeight = 10, int segment = 10, double angle = 360, double radius = 100, double angleHeight = 90)
        {
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
                    IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(meshIn, hit_tid, ray);
                    hit_dist = origin.Distance(ray.PointAt(intr.RayParameter));
                    #endregion

                    #region 判定是否在距离内，如果是，加上先前面积
                    if (hit_dist <= radius)
                    {
                        var triArea = meshIn.GetTriArea(hit_tid);
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
                                hitIndexDic.TryAdd(hit_vid, 1);
                            }
                        }

                        rayArea += triArea;
                    }
                    #endregion
                }
                #endregion
            };

            return rayArea;
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
        public static Vector3d[] CreateSphereDirection(Vector3d origin, int segmentHeight, int segment, double angle, double radius,double angleHeight)
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
                    vertices[index]=new Vector3d(new Vector3d(origin.x + _x, origin.y + _y, origin.z + _z)-origin);
                    index++;
                }
            }

            return vertices;
        }

        public static DMesh3 ApplyColorsBasedOnRays(DMesh3 mesh, Dictionary<int, int> hitTrianglesDic, Colorf originColor, Colorf DestnationColor)
        {
            DMesh3 meshIn = new DMesh3(mesh);
            var maxNumber = hitTrianglesDic.Values.Max();
            
            foreach (var item in hitTrianglesDic)
            {
                var tempColor= Colorf.Lerp(originColor, DestnationColor, (float)item.Value / maxNumber);
                meshIn.SetVertexColor(item.Key, tempColor);
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


        public static double CalcVisibilityPercentParallel(double visibleArea, double wholeArea)
        {
            var result = visibleArea / wholeArea;
            return result;
        }
        #endregion

        #region 004_Generating polyline

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
        public static void CreateBrepMinusTopBtn(Rh.Brep single, Rh.MeshingParameters mp, out Rh.Mesh resultTopBtn, out Rh.Mesh resultSides, out double size, out Rh.Point3d centPt)
        {
            var faceList = single.Faces;
            var heightList = new List<double>(faceList.Count);
            var indexList = new List<int>(faceList.Count);
            for (int j = 0; j < faceList.Count; j++)
            {
                var singleFace = faceList[j];
                var boundingBox = singleFace.GetBoundingBox(false);
                heightList.Add(boundingBox.Center.Z);
                indexList.Add(j);
            }
            var max = heightList.Max();
            var min = heightList.Min();
            var indexMax = heightList.IndexOf(max);
            var indexMin = heightList.IndexOf(min);
            indexList.Remove(indexMax);
            indexList.Remove(indexMin);

            //输出 四角面top btn
            Rh.Mesh topBtnMesh = new Rh.Mesh();
            var topMesh = single.Faces[indexMax];
            topMesh.ShrinkFace(Rh.BrepFace.ShrinkDisableSide.ShrinkAllSides);
            var btnMesh = single.Faces[indexMin];
            btnMesh.ShrinkFace(Rh.BrepFace.ShrinkDisableSide.ShrinkAllSides);

            topBtnMesh.Append(Rh.Mesh.CreateFromSurface(topMesh,mp));
            topBtnMesh.Append(Rh.Mesh.CreateFromSurface(btnMesh, mp)); 
            resultTopBtn = topBtnMesh;

            //输出 三角面side
            Rh.Brep result = new Rh.Brep();
            var sumArea = 0d;
            for (int k = 0; k < indexList.Count; k++) { 
                var tempBrep = single.Faces[indexList[k]].ToBrep();
                sumArea += tempBrep.GetArea();
                result.Append(tempBrep);
            }

            var tempMeshArray = Rh.Mesh.CreateFromBrep(result, mp);
            var sideMesh = new Rh.Mesh();
            sideMesh.Append(tempMeshArray);
            sideMesh.Faces.ConvertQuadsToTriangles();
            resultSides = sideMesh;

            //输出中心点
            centPt = single.GetBoundingBox(false).Center;
            
            //输出面积
            size = sumArea;
        }

        public static List<Vector3d> NTSPtList2Vector3dList(IEnumerable<NTS.Geometries.Point> ptList)
        {
            List<Vector3d> vectorList = new List<Vector3d>(ptList.Count());
            for (int i = 0; i < ptList.Count(); i++)
                vectorList.Add(NTSPt2Vector3d(ptList.ElementAt(i)));
            return vectorList;
        }

        public static Vector3d NTSPt2Vector3d(NTS.Geometries.Point NTSPt, double height=0)
        {
            return new Vector3d(NTSPt.X, NTSPt.Y, height);
        }

        public static Vector3d NTSPt2Vector3d(NTS.Geometries.Point NTSPt)
        {
            return new Vector3d(NTSPt.X, NTSPt.Y, NTSPt.Z);
        }

        private static NTS.Geometries.Point ConvertFromRh_Point(Rh.Point3d pt)
        {
            return new NTS.Geometries.Point(pt.X, pt.Y, pt.Z);
        }

        private static NTS.Geometries.Point ConvertFromRh_Point2D(Rh.Point3d pt)
        {
            return new NTS.Geometries.Point(pt.X, pt.Y, 0);
        }

        public static NTS.Geometries.Point[] ConvertFromRh_Point( IEnumerable<Rh.Point3d> pts)
        {
            var count = pts.Count();
            NTS.Geometries.Point[] ptResult = new NTS.Geometries.Point[count]; 
            for (int i = 0; i < count; i++)
            {
                ptResult[i]= new NTS.Geometries.Point(pts.ElementAt(i).X, pts.ElementAt(i).Y, pts.ElementAt(i).Z);
            }
            return ptResult;
        }

        public static Dictionary<NetTopologySuite.Geometries.Point, double> GenerateDic(double[] areaList, Rh.Point3d[] ptList)
        {
            Dictionary<NetTopologySuite.Geometries.Point, double> ptDic = new Dictionary<NTS.Geometries.Point, double>();
            for (int i = 0; i < areaList.Length; i++)
            {
                var pt2d = ConvertFromRh_Point2D(ptList[i]);
                if (ptDic.ContainsKey(pt2d))
                {
                    ptDic[pt2d] += areaList[i];
                }
                else
                {
                    ptDic.Add(pt2d, areaList[i]);
                }
                
            }
            return ptDic;
        }

        public static Rh.Mesh ConvertFromDMesh3(DMesh3 meshInput)
        {
            Rh.Mesh meshOutput = new Rh.Mesh();

            var tempMeshInputVertices = meshInput.Vertices().ToArray();
            var rhVerticesList = ConvertFromDMeshVector(tempMeshInputVertices);
            var rhFacesList= ConvertFromDMeshTri(meshInput.Triangles().ToArray());
            
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
            var tempColor= ConvertColorValueBasedFloat(vertexColor.x, vertexColor.y, vertexColor.z);
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
            colorResult[0]= Math.Max(0, Math.Min(255, (int)Math.Floor(r * 256.0)));
            colorResult[1]= Math.Max(0, Math.Min(255, (int)Math.Floor(g * 256.0)));
            colorResult[2]= Math.Max(0, Math.Min(255, (int)Math.Floor(b * 256.0)));
            return colorResult;
        }

        class MyException : Exception
        {
            public MyException(string message) : base(message)
            {
            }
        }
        #endregion
    }

    public class GeneratedMeshClass
    {
        public Rh.Mesh topBtnList { get; set; }
        public DMesh3 sideList { get; set; }

        public double[] sideAreaList { get; set; }
        public Rh.Point3d[] cenPtList { get; set; }

        public GeneratedMeshClass(Rh.Mesh TopBtnList, DMesh3 SideList, double[] SideAreaList, Rh.Point3d[] CenPtList)
        {
            topBtnList = TopBtnList;
            sideList = SideList;
            sideAreaList = SideAreaList;
            cenPtList = CenPtList;
        }

        public GeneratedMeshClass() { }
    }




}
