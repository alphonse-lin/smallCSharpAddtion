using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using g3;
using SketchUpNET;

namespace UrbanX.Application.Geometry
{
    public class SketchUpManager
    {
        public static SketchUp LoadFromSkp(string filePath)
        {
            SketchUp skp = new SketchUp();
            skp.LoadModel(filePath);
            return skp;
        }

        public static string WriteSUModel(SketchUp skp, string filePath, string version = "2013")
        {
            var result = "Export Error";
            if (version=="2020")
            {
                skp.WriteNewModel(filePath);
                result = "export as SU2020";
            }
            else
            {
                var defaultPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string skpPath = Path.Combine(defaultPath, "skpModel_temp_2020.skp");
                skp.WriteNewModel(skpPath);
                ReformatModel(skpPath, filePath, version);
                System.IO.File.Delete(skpPath);
                result = $"export as {version}";
            }
            return result;
        }

        public static bool ReformatModel(string filepath, string newfilepath, string version)
        {
            SketchUpNET.SketchUp skp = new SketchUpNET.SketchUp();
            SKPVersion v = SKPVersion.V2020;
            switch (version)
            {
                case "2013": v = SKPVersion.V2013; break;
                case "2014": v = SKPVersion.V2014; break;
                case "2015": v = SKPVersion.V2015; break;
                case "2016": v = SKPVersion.V2016; break;
                case "2017": v = SKPVersion.V2017; break;
                case "2018": v = SKPVersion.V2018; break;
                case "2019": v = SKPVersion.V2019; break;
                case "2020": v = SKPVersion.V2020; break;
            }
            return skp.SaveAs(filepath, v, newfilepath);
        }

        public static SketchUp ExtrudeSUModelFromData(Vector3d[][] polygonData, double[] height, double[][] envelopes)
        {
            SketchUp skp = new SketchUp();
            skp.Components = new Dictionary<string, Component>();
            skp.Curves = new List<Curve>();
            skp.Edges = new List<Edge>();
            skp.Groups = new List<Group>();
            skp.Instances = new List<Instance>();
            skp.Layers = new List<Layer>();
            skp.Materials = new Dictionary<string, Material>();
            skp.Surfaces = new List<Surface>();

            var savedLayer = new Layer("LayerTest");
            skp.Layers.Add(savedLayer);

            var allgroup = new List<Group>(polygonData.Length);
            for (int i = 0; i < polygonData.Length; i++)
            {
                Group group = new Group();
                var edgeBtmList = GenerateBtmEdges(polygonData[i]);
                var edgeWholeList = GenerateAllEdges(edgeBtmList, height[i]);
                var srfWholeList = GenerateSurfaces(edgeBtmList, height[i]);

                //group.Edges = edgeWholeList;
                skp.Surfaces.AddRange(srfWholeList);
                //group.Curves = new List<Curve>();
                //group.Groups = new List<Group>() { group };
                //group.Instances = new List<Instance>();
                //group.Surfaces = new List<Surface>();
                //group.Edges = new List<Edge>();
                //group.Transformation = new Transform();

                //group.Name = i.ToString();
                //group.Surfaces.AddRange(srfWholeList);
                //group.Edges.AddRange(edgeWholeList);
                //group.Transformation = new Transform(CreateTransformationData(envelopes[i][0], envelopes[i][1], height[i]));
                ////group.Transformation = new Transform(CreateTransformationData(envelopes[i][2], envelopes[i][3], height[i]));
                //group.Layer = "LayerTest";

                //allgroup.Add(group);
                ////skp.Edges.AddRange(edgeWholeList);
            }
            skp.Groups = allgroup;

            return skp;
        }

        public static void ExtrudeSUModelFromData(Vector3d[][] polygonData,double[] height,bool saveModel, string filePath)
        {
            SketchUp skp = new SketchUp();
            skp.Surfaces = new List<Surface>();
            skp.Edges = new List<Edge>();

            for (int i = 0; i < polygonData.Length; i++)
            {
                var edgeBtmList=GenerateBtmEdges(polygonData[i]);
                //var edgeWholeList = GenerateAllEdges(edgeBtmList, height[i]);
                var srfWholeList = GenerateSurfaces(edgeBtmList, height[i]);

                skp.Surfaces.AddRange(srfWholeList);
                //skp.Edges.AddRange(edgeWholeList);
            }
        }

        private static void GenerateEdges(SketchUp skp,Vector3d[] ptList)
        {
            for (int i = 0; i <ptList.Length-1; i++)
            {
                var ptS = ptList[i];
                Vertex vertexS = new Vertex(ptS.x, ptS.y, ptS.z);
                var ptE = ptList[i+1];
                Vertex vertexE = new Vertex(ptE.x, ptE.y, ptE.z);

                Edge edge = new Edge(vertexS, vertexE,"LayerTest");
                skp.Edges.Add(edge);
            }
        }

        private static List<Surface> GenerateSurfaces(List<Edge> edgeResult, double height)
        {
            List<Surface> srfs = new List<Surface>(edgeResult.Count+2);
            List<Surface> srf_sides = new List<Surface>(edgeResult.Count);
            
            Surface srf_btm = new Surface();
            Surface srf_top = new Surface();
            srf_btm.Vertices = new List<Vertex>(edgeResult.Count);
            srf_top.Vertices = new List<Vertex>(edgeResult.Count);

            for (int i = 0; i < edgeResult.Count; i++)
            {
                srf_btm.Vertices.Add(edgeResult[i].Start);
                srf_top.Vertices.Add(new Vertex(edgeResult[i].Start.X, edgeResult[i].Start.Y, edgeResult[i].Start.Z+height));
            }
            srf_btm.OuterEdges = ExtractLoopFromPt(srf_btm.Vertices);
            srf_top.OuterEdges = ExtractLoopFromPt(srf_top.Vertices);
            srf_btm.Normal = new Vector(0, 0, 1);

            for (int i = 0; i < srf_btm.Vertices.Count; i++)
            {
                Surface srf_side = new Surface();
                srf_side.Vertices = new List<Vertex>(4);
                List<Vertex> vertexList = new List<Vertex>(4);
                if (i!= srf_btm.Vertices.Count-1)
                {
                    vertexList = new List<Vertex>(4)
                    {
                    srf_btm.Vertices[i],
                    srf_btm.Vertices[i+1],
                    srf_top.Vertices[i+1],
                    srf_top.Vertices[i],
                    };
                }
                else
                {
                    vertexList = new List<Vertex>(4)
                    {
                    srf_btm.Vertices[i],
                    srf_btm.Vertices[0],
                    srf_top.Vertices[0],
                    srf_top.Vertices[i],
                    };
                }
                
                srf_side.Vertices=vertexList;
                srf_side.OuterEdges = ExtractLoopFromPt(srf_side.Vertices);
                srf_sides.Add(srf_side);
            }

            srfs.Add(ReverseSrf(srf_btm));
            srfs.AddRange(srf_sides);
            srfs.Add(srf_top);

            return srfs;
        }

        private static List<Edge> GenerateBtmEdges(Vector3d[] ptList)
        {
            List<Edge> edgeList = new List<Edge>(ptList.Length);
            for (int i = 0; i < ptList.Length-1; i++)
            {
                Surface srf_1 = new Surface();
                srf_1.Vertices = new List<Vertex>();

                var pt_1 = ptList[i];
                Vertex vertexS = new Vertex(pt_1.x, pt_1.y, pt_1.z);
                var pt_2 = ptList[i + 1];
                Vertex vertexE = new Vertex(pt_2.x, pt_2.y, pt_2.z);

                Edge edge = new Edge(vertexS, vertexE, "LayerTest");
                edgeList.Add(edge);
            }
            return edgeList;
        }

        private static List<Edge> GenerateAllEdges(List<Edge> edge_btm,double height)
        {
            List<Edge> edges = new List<Edge>(edge_btm.Count*3);
            List<Edge> edge_sides = new List<Edge>(edge_btm.Count);
            List<Edge> edge_top = new List<Edge>(edge_btm.Count);

            for (int i = 0; i < edge_btm.Count; i++)
            {
                edge_top.Add(new Edge(
                    new Vertex(edge_btm[i].Start.X, edge_btm[i].Start.Y, edge_btm[i].Start.Z+ height),
                    new Vertex(edge_btm[i].End.X, edge_btm[i].End.Y, edge_btm[i].Start.Z + height),
                    "LayerTest"
                    ));
            }

            for (int i = 0; i < edge_btm.Count; i++)
            {
                Edge edge_side = new Edge(
                    new Vertex(edge_btm[i].Start.X, edge_btm[i].Start.Y, edge_btm[i].Start.Z), 
                    new Vertex(edge_top[i].Start.X, edge_top[i].Start.Y, edge_top[i].Start.Z),
                    "LayerTest"
                    );
                edge_sides.Add(edge_side);
            }

            edges.AddRange(edge_btm);
            edges.AddRange(edge_top);
            edges.AddRange(edge_sides);

            return edges;
        }

        private static Surface ReverseSrf(Surface srf)
        {
            Surface srfNew = srf;
            srfNew.Vertices.Reverse();
            return srfNew;
        }

        private static Loop ExtractLoopFromPt(List<Vertex> ptList)
        {
            List<Edge> edgeList = new List<Edge>(ptList.Count);
            for (int i = 0; i < ptList.Count- 1; i++)
            {
                Surface srf_1 = new Surface();
                srf_1.Vertices = new List<Vertex>();

                var pt_1 = ptList[i];
                Vertex vertexS = new Vertex(pt_1.X, pt_1.Y, pt_1.Z);
                var pt_2 = ptList[i + 1];
                Vertex vertexE = new Vertex(pt_2.X, pt_2.Y, pt_2.Z);

                Edge edge = new Edge(vertexS, vertexE, "LayerTest");
                edgeList.Add(edge);
            }
            
            return new Loop(edgeList);
        }

        private static double[] CreateTransformationData(double x, double y,double z, double scale=1d)
        {
            double[] transData = new double[16];
            transData = new double[]
            {
                1,0,0,0,
                0,1,0,0,
                0,0,1,0,
                x,y,z,scale
            };
            return transData;
        }
    }
}