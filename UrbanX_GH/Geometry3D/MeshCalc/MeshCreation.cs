using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using g3;
using gs;
using System.IO;
using NTS = NetTopologySuite;
using Rh=Rhino.Geometry;

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

        public static double[] CalcRaysGetArea(DMesh3 mesh, IEnumerable<Vector3d> originList, int segmentHeight = 10, int segment = 10, double angle = 360, double radius = 100, double angleHeight = 90)
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
                            var triArea=meshIn.GetTriArea(hit_tid);
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
        public static Vector3d[] CreateSphereDirection(Vector3d origin, int segmentHeight, int segment, double angle, double radius,double angleHeight)
        {
            if (angleHeight > 90)
                angleHeight = 90;
            if (angle > 360)
                angle = 360;
            double _angleHeight = Math.PI * angleHeight / 180 / segment;
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
        public static DMesh3 ConvertFromRh_Meshes(Rh.Mesh[] meshList, out Dictionary<NTS.Geometries.Point, double> centerPtDic)
        {
            Dictionary<NTS.Geometries.Point, double> temp_centerPtDic = new Dictionary<NTS.Geometries.Point, double>();
            DMesh3 meshCollection = new DMesh3();

            for (int i = 0; i < meshList.Length; i++)
            {
                var singleMesh = meshList[i];
                //求出转换后的DMesh3 
                DMesh3 tempMesh = ConvertFromRh_Mesh(singleMesh);
                MeshEditor.Append(meshCollection, tempMesh);

                //求出中点
                var centerPt = ConvertFromRh_Point(singleMesh.GetBoundingBox(false).Center);
                //求出面积
                var tempArea = Rh.AreaMassProperties.Compute(singleMesh).Area;

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

        public static Rh.Brep CreateBrepMinusTopBtn(Rh.Brep single, out double size)
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
            indexList.Remove(heightList.IndexOf(max));
            indexList.Remove(heightList.IndexOf(min));

            Rh.Brep result = new Rh.Brep();
            for (int k = 0; k < indexList.Count; k++)
                result.Append(single.Faces[indexList[k]].ToBrep());


            return result;
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

        public static DMesh3[] ConvertFromRh_Meshes(Rh.Mesh[] meshList)
        {
            DMesh3[] result = new DMesh3[meshList.Length];
            for (int i = 0; i < meshList.Length; i++)
            {
                var singleMesh = meshList[i] ;
                result[i] = ConvertFromRh_Mesh(singleMesh);
            }
            return result;
        }

        public static DMesh3 ConvertFromRh_Mesh(Rh.Mesh[] meshList)
        {
            DMesh3 meshCollection = new DMesh3();
            for (int i = 0; i < meshList.Length; i++)
            {
                var singleMesh = meshList[i];
                MeshEditor.Append(meshCollection, ConvertFromRh_Mesh(singleMesh));
            }
            return meshCollection;
        }

        private static NTS.Geometries.Point ConvertFromRh_Point(Rh.Point3d pt)
        {
            return new NTS.Geometries.Point(pt.X, pt.Y, pt.Z);
        }

        public static DMesh3 ConvertFromRh_Mesh(Rh.Mesh Rh_mesh)
        {
            var Rh_vertex = Rh_mesh.Vertices;
            var Rh_tri = Rh_mesh.Faces;
            var Rh_normals = Rh_mesh.Normals;

            return  DMesh3Builder.Build(Rh_vertex, Rh_tri, Rh_normals);
        }
        #endregion
    }
}
