using System;
using System.Collections.Generic;
using g3;
using UrbanX.Application.Geometry;
using System.Linq;

namespace Urbanx.Application.Geometry.Extension
{
    public static class Extension
    {
        public static Vector3d Tog3Pt(this PlanktonVertex ptIn)
        {
            return new Vector3d(ptIn.X, ptIn.Y, ptIn.Z);
        }

        public static Vector3d ClosestPoint(this DCurve3 curve, Vector3d pt, out int crvIndex, out double crvIntr)
        {
            int segCount = curve.SegmentCount;
            Vector3d closestPt = new Vector3d();

            double minCrvDis = double.MaxValue;
            double minCrvIntr = -1;
            int minCrvIndex = -1;

            //Segment3d minSeg = new Segment3d();
            //double tempCrvLength = 0;

            List<double> allLength = new List<double>(segCount);

            for (int i = 0; i < segCount; i++)
            {
                var segment = curve.GetSegment(i);

                var tempPt = segment.ClosestPoint(pt, out double tempCrvIntr, out double distanceSquared3D);
                //allLength.Add(distanceSquared2D);

                if (distanceSquared3D <= minCrvDis)
                {
                    closestPt = tempPt;
                    minCrvDis = distanceSquared3D;
                    minCrvIntr = tempCrvIntr;
                    minCrvIndex = i;
                    //minSeg = segment;
                }
            }

            //for (int i = 0; i < minCrvIndex; i++)
            //    tempCrvLength += allLength[i];
            //crvIntr = (minCrvIntr * minSeg.Length+ tempCrvLength) / allLength.Sum();

            crvIndex = minCrvIndex;
            crvIntr = minCrvIntr;

            return closestPt;
        }

        private static Vector3d ClosestPoint(this Segment3d curve, Vector3d pt, out double crvIntr, out double distanceSquared3D)
        {
            Vector3d p1 = curve.P0;
            Vector3d p2 = curve.P0;

            double dx = p2.x - p1.x;
            double dy = p2.y - p1.y;
            double dz = p2.z - p1.z;

            Vector3d intrPt = new Vector3d();
            if ((dx == 0) && (dy == 0) && (dz == 0))
            {
                // It's a point not a line segment.
                intrPt = p1;
                crvIntr = 0;
                //distanceSquared2D = 0;
                distanceSquared3D = 0;
                return intrPt;
            }

            // Calculate the t that minimizes the distance.
            double t = ((pt.x - p1.x) * dx + (pt.y - p1.y) * dy + (pt.z - p1.z) * dz) /
              (dx * dx + dy * dy + dz * dz);



            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                intrPt = new Vector3d(p1.x, p1.y, p1.z);
                crvIntr = 0;
            }
            else if (t > 1)
            {
                intrPt = new Vector3d(p2.x, p2.y, p2.z);
                crvIntr = 1;
            }
            else
            {
                intrPt = new Vector3d(p1.x + t * dx, p1.y + t * dy, p1.z + t * dz);
                crvIntr = t;
            }


            //distanceSquared2D = new Vector2d(p1.x, p1.y).Distance(new Vector2d(p2.x, p2.y));
            distanceSquared3D = intrPt.Distance(pt);
            return intrPt;
        }

        public static bool SetVertex(this PlanktonVertexList vertices, int v, Vector3d pt)
        {
            return vertices.SetVertex(v, pt.x, pt.y, pt.z);
        }

        public static int Add(this PlanktonVertexList vertices, Vector3d vec)
        {
            return vertices.Add(vec.x, vec.y, vec.z);
        }

        public static Vector3d Tog3Pt(this PlanktonXYZ ptIn)
        {
            return new Vector3d(ptIn.X, ptIn.Y, ptIn.Z);
        }

        public static int[] ConnectedVertices(this DMesh3 source, int vIndex)
        {
            var edgesNeighbours = source.VtxVerticesItr(vIndex).ToArray();
            //List<int> indexNeighbours = new List<int>();

            //for (int edgeIndex = 0; edgeIndex < edgesNeighbours.Count; edgeIndex++)
            //{
            //    var tempEdge = source.GetEdge(edgeIndex);
            //    var vertex01 = tempEdge.a;
            //    var vertex02 = tempEdge.b;
            //    if (!indexNeighbours.Contains(vertex01))
            //        indexNeighbours.Add(vertex01);
            //    if (!indexNeighbours.Contains(vertex02))
            //        indexNeighbours.Add(vertex02);
            //}
            return edgesNeighbours;
        }

        public static PlanktonMesh g3Mesh2pMesh(this DMesh3 source)
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            //source.Vertices.CombineIdentical(true, true);
            //source.Vertices.CullUnused();
            //source.UnifyNormals();
            //source.Weld(Math.PI);

            var vertices = source.Vertices();
            foreach (Vector3d v in vertices)
            {
                pMesh.Vertices.Add(v);
            }

            for (int i = 0; i < source.TriangleCount; i++)
            {
                pMesh.Faces.Add(new PlanktonFace());
            }

            for (int i = 0; i < source.EdgeCount; i++)
            {
                PlanktonHalfedge HalfA = new PlanktonHalfedge();
                var edgeFromSource = source.GetEdge(i);

                //To Do 检查合理性
                HalfA.StartVertex = edgeFromSource.a;
                if (pMesh.Vertices[HalfA.StartVertex].OutgoingHalfedge == -1)
                    pMesh.Vertices[HalfA.StartVertex].OutgoingHalfedge = pMesh.Halfedges.Count;

                PlanktonHalfedge HalfB = new PlanktonHalfedge();
                HalfB.StartVertex = edgeFromSource.b;
                if (pMesh.Vertices[HalfB.StartVertex].OutgoingHalfedge == -1)
                    pMesh.Vertices[HalfB.StartVertex].OutgoingHalfedge = pMesh.Halfedges.Count + 1;

                int[] ConnectedFaces = new int[] { edgeFromSource.c, edgeFromSource.d };
                bool[] Match = new bool[] { source.tri_has_neighbour_t(i, edgeFromSource.c), source.tri_has_neighbour_t(i, edgeFromSource.d) };

                Match[0] = false;
                if (Match.Length > 1) Match[1] = true;

                int VertA = source.GetTriangle(ConnectedFaces[0]).a;
                int VertB = source.GetTriangle(ConnectedFaces[0]).b;
                int VertC = source.GetTriangle(ConnectedFaces[0]).c;

                if ((VertA == HalfA.StartVertex && VertB == HalfB.StartVertex)) { Match[0] = true; }
                if ((VertB == HalfA.StartVertex && VertC == HalfB.StartVertex)) { Match[0] = true; }
                if ((VertC == HalfA.StartVertex && VertA == HalfB.StartVertex)) { Match[0] = true; }

                if (Match[0] == true)
                {
                    HalfA.AdjacentFace = ConnectedFaces[0];
                    if (pMesh.Faces[HalfA.AdjacentFace].FirstHalfedge == -1)
                    {
                        pMesh.Faces[HalfA.AdjacentFace].FirstHalfedge = pMesh.Halfedges.Count;
                    }
                    if (ConnectedFaces.Length > 1)
                    {
                        HalfB.AdjacentFace = ConnectedFaces[1];
                        if (pMesh.Faces[HalfB.AdjacentFace].FirstHalfedge == -1)
                        {
                            pMesh.Faces[HalfB.AdjacentFace].FirstHalfedge = pMesh.Halfedges.Count + 1;
                        }
                    }
                    else
                    {
                        HalfB.AdjacentFace = -1;
                        pMesh.Vertices[HalfB.StartVertex].OutgoingHalfedge = pMesh.Halfedges.Count + 1;
                    }
                }
                else
                {
                    HalfB.AdjacentFace = ConnectedFaces[0];
                    if (pMesh.Faces[HalfB.AdjacentFace].FirstHalfedge == -1)
                    {
                        pMesh.Faces[HalfB.AdjacentFace].FirstHalfedge = pMesh.Halfedges.Count + 1;
                    }

                    if (ConnectedFaces.Length > 1)
                    {
                        HalfA.AdjacentFace = ConnectedFaces[1];

                        if (pMesh.Faces[HalfA.AdjacentFace].FirstHalfedge == -1)
                        {
                            pMesh.Faces[HalfA.AdjacentFace].FirstHalfedge = pMesh.Halfedges.Count;
                        }
                    }
                    else
                    {
                        HalfA.AdjacentFace = -1;
                        pMesh.Vertices[HalfA.StartVertex].OutgoingHalfedge = pMesh.Halfedges.Count;
                    }
                }
                pMesh.Halfedges.Add(HalfA);
                pMesh.Halfedges.Add(HalfB);

            }

            for (int i = 0; i < pMesh.Halfedges.Count; i += 2)
            {
                int[] EndNeighbours = source.ConnectedVertices(pMesh.Halfedges[i + 1].StartVertex);
                for (int j = 0; j < EndNeighbours.Length; j++)
                {
                    if (EndNeighbours[j] == pMesh.Halfedges[i].StartVertex)
                    {
                        int EndOfNextHalfedge = EndNeighbours[(j - 1 + EndNeighbours.Length) % EndNeighbours.Length];
                        int StartOfPrevOfPairHalfedge = EndNeighbours[(j + 1) % EndNeighbours.Length];

                        int NextEdge = source.FindEdge(pMesh.Halfedges[i + 1].StartVertex, EndOfNextHalfedge);
                        int PrevPairEdge = source.FindEdge(pMesh.Halfedges[i + 1].StartVertex, StartOfPrevOfPairHalfedge);

                        if (source.GetEdge(NextEdge).a == pMesh.Halfedges[i + 1].StartVertex)
                        {
                            pMesh.Halfedges[i].NextHalfedge = NextEdge * 2;
                        }
                        else
                        {
                            pMesh.Halfedges[i].NextHalfedge = NextEdge * 2 + 1;
                        }

                        if (source.GetEdge(PrevPairEdge).b == pMesh.Halfedges[i + 1].StartVertex)
                        {
                            pMesh.Halfedges[i + 1].PrevHalfedge = PrevPairEdge * 2;
                        }
                        else
                        {
                            pMesh.Halfedges[i + 1].PrevHalfedge = PrevPairEdge * 2 + 1;
                        }
                        break;
                    }
                }

                int[] StartNeighbours = source.ConnectedVertices(pMesh.Halfedges[i].StartVertex);
                for (int j = 0; j < StartNeighbours.Length; j++)
                {
                    if (StartNeighbours[j] == pMesh.Halfedges[i + 1].StartVertex)
                    {
                        int EndOfNextOfPairHalfedge = StartNeighbours[(j - 1 + StartNeighbours.Length) % StartNeighbours.Length];
                        int StartOfPrevHalfedge = StartNeighbours[(j + 1) % StartNeighbours.Length];

                        int NextPairEdge = source.FindEdge(pMesh.Halfedges[i].StartVertex, EndOfNextOfPairHalfedge);
                        int PrevEdge = source.FindEdge(pMesh.Halfedges[i].StartVertex, StartOfPrevHalfedge);

                        if (source.GetEdge(NextPairEdge).a == pMesh.Halfedges[i].StartVertex)
                        {
                            pMesh.Halfedges[i + 1].NextHalfedge = NextPairEdge * 2;
                        }
                        else
                        {
                            pMesh.Halfedges[i + 1].NextHalfedge = NextPairEdge * 2 + 1;
                        }

                        if (source.GetEdge(PrevEdge).b == pMesh.Halfedges[i].StartVertex)
                        {
                            pMesh.Halfedges[i].PrevHalfedge = PrevEdge * 2;
                        }
                        else
                        {
                            pMesh.Halfedges[i].PrevHalfedge = PrevEdge * 2 + 1;
                        }
                        break;
                    }
                }
            }
            return pMesh;

        }

        public static DMesh3 pMesh2g3Mesh(this PlanktonMesh meshIn)
        {
            DMesh3 meshOut = new DMesh3();
            foreach (PlanktonVertex vertex in meshIn.Vertices)
            {
                meshOut.AppendVertex(vertex.Tog3Pt());
            }
            for (int i = 0; i < meshIn.Faces.Count; i++)
            {
                int[] faceVertices = meshIn.Faces.GetFaceVertices(i);
                if (faceVertices.Length == 3)
                {
                    meshOut.AppendTriangle(faceVertices[0], faceVertices[1], faceVertices[2]);
                }
                #region 大于三面，目前不可能出现
                else if (faceVertices.Length == 4)
                {
                    meshOut.AppendTriangle(faceVertices[0], faceVertices[1], faceVertices[2], faceVertices[3]);
                }
                else if (faceVertices.Length > 4)
                {
                    PlanktonXYZ faceCenter = meshIn.Faces.GetFaceCenter(i);
                    meshOut.AppendVertex(new Vector3d(faceCenter.X, faceCenter.Y, faceCenter.Z));
                    for (int j = 0; j < faceVertices.Length; j++)
                    {
                        meshOut.AppendTriangle(faceVertices[j], faceVertices[(j + 1) % faceVertices.Length], meshOut.VertexCount - 1);
                    }
                }
                #endregion
            }
            return new DMesh3(meshOut,true);
        }
    }
}
