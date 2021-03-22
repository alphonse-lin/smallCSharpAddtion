using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using SketchUpNET;

namespace UrbanX.Application.Geometry
{
    public class SketchUpManager
    {
        public static SketchUp GetMeshFromSkp(string filePath)
        {
            SketchUp skp = new SketchUp();
            skp.LoadModel(filePath);
            return skp;
        }

        public static bool WriteSUModel(SketchUp skp, string filePath)
        {
            return skp.WriteNewModel(filePath);
        }

        public static void ExtrudeSUModelFromData(Vector3d[][] polygonData,double[] height)
        {
            SketchUp skp = new SketchUp();
            skp.Surfaces = new List<Surface>();
            skp.Edges = new List<Edge>();
        }

        private static void GenerateEdges(SketchUp skp,Vector3d[] ptList)
        {
            for (int i = 0; i <ptList.Length-1; i++)
            {
                var ptS = ptList[i];
                Vertex vertexS = new Vertex(ptS.x, ptS.y, ptS.z);
                var ptE = ptList[i+1];
                Vertex vertexE = new Vertex(ptE.x, ptE.y, ptE.z);

                Edge edge = new Edge(vertexS, vertexE,"default");
                skp.Edges.Add(edge);
            }
        }

        private static void GenerateSurfaces(SketchUp skp, Vector3d[] ptList)
        {
            var edgeResult = GenerateEdges(ptList);
            for (int i = 0; i < edgeResult.Count; i++)
            {
                Surface srf_1 = new Surface();
                srf_1.Vertices = new List<Vertex>();

                edgeResult[i].Start

                Edge edge = new Edge(vertexS, vertexE, "default");
                skp.Edges.Add(edge);
            }
        }

        private static List<Edge> GenerateEdges(Vector3d[] ptList)
        {
            List<Edge> edgeList = new List<Edge>(ptList.Length-1);
            for (int i = 0; i < ptList.Length - 1; i++)
            {
                Surface srf_1 = new Surface();
                srf_1.Vertices = new List<Vertex>();

                var pt_1 = ptList[i];
                Vertex vertexS = new Vertex(pt_1.x, pt_1.y, pt_1.z);
                var pt_2 = ptList[i + 1];
                Vertex vertexE = new Vertex(pt_2.x, pt_2.y, pt_2.z);

                Edge edge = new Edge(vertexS, vertexE, "default");
                edgeList.Add(edge);
            }
            return edgeList;
        }
    }
}