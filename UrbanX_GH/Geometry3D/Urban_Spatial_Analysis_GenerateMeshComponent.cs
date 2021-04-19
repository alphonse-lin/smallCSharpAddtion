using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

using UrbanX.Planning.IndexCalc;
using UrbanX.Planning.UrbanDesign;
using UrbanX_GH.Application.Geometry;
using UrbanX_GH.Properties;


// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace UrbanX_GH
{
    public class UrbanX_Sustainability_MeshSub_Component : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public XElement meta;
        public static string c_id = "Urban_Spatial_Analysis_GenerateMesh";
        public static string c_moduleName = "Urban_Spatial_Analysis";

        #region 备用
        //public Urban_SustainabilityComponent()
        //  : base("IndexCalculation", "IndexCalc",
        //      "index calculation, included EC,WC, GC, Population Amount",
        //      "UrbanXFireFly", "AutoGenerator")
        //{
        //}
        #endregion
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public UrbanX_Sustainability_MeshSub_Component() : base("", "", "", "", "")
        {
            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(SharedUtils.Resolve);
            this.meta = SharedResources_Utils.GetXML(c_moduleName, c_id);
            this.Name = this.meta.Element("name").Value;
            this.NickName = this.meta.Element("nickname").Value;
            this.Description = this.meta.Element("description").Value + "\nv.1";
            this.Category = this.meta.Element("category").Value;
            this.SubCategory = this.meta.Element("subCategory").Value;
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            this.meta = SharedResources_Utils.GetXML(c_moduleName, c_id);
            List<XElement> list = this.meta.Element("inputs").Elements("input").ToList<XElement>();
            pManager.AddGenericParameter((string)list[0].Attribute("name"), (string)list[0].Attribute("nickname"), (string)list[0].Attribute("description"), GH_ParamAccess.list);
            pManager.AddNumberParameter((string)list[1].Attribute("name"), (string)list[1].Attribute("nickname"), (string)list[1].Attribute("description"), GH_ParamAccess.item,-1);
            pManager[1].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            this.meta = SharedResources_Utils.GetXML(c_moduleName, c_id);
            List<XElement> list = this.meta.Element("outputs").Elements("output").ToList<XElement>();
            pManager.AddGenericParameter((string)list[0].Attribute("name"), (string)list[0].Attribute("nickname"), (string)list[0].Attribute("description"), GH_ParamAccess.item);
            pManager.AddGenericParameter((string)list[1].Attribute("name"), (string)list[1].Attribute("nickname"), (string)list[1].Attribute("description"), GH_ParamAccess.list);
            pManager.AddGenericParameter((string)list[2].Attribute("name"), (string)list[2].Attribute("nickname"), (string)list[2].Attribute("description"), GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> brepInList = new List<Brep>();
            double gridSize = -1;
            if (!DA.GetDataList<Brep>(0, brepInList)) { return; }
            if (!DA.GetData(1, ref gridSize)) { return; }

            #region 细分mesh
            Brep[] brepIn = brepInList.ToArray();
            var meshOut = new Mesh[brepIn.Length];
            var sizeList = new double[brepIn.Length];
            var cenPtList = new Point3d[brepIn.Length];
            
            System.Threading.Tasks.Parallel.For(0, brepIn.Length, i =>
            {
                var singleBrep = MeshCreation.CreateBrepMinusTopBtn(brepIn[i], out double size, out Point3d cenPt);
                if (gridSize == -1)
                    gridSize = RhinoToolManager.GetMaxBounds(brepIn[i])/3;
                
                var mp = MeshingParameters.Default;
                mp.MaximumEdgeLength = gridSize;
                mp.MinimumEdgeLength = gridSize;
                mp.GridAspectRatio = 1;

                var tempMeshArray=Mesh.CreateFromBrep(singleBrep, mp);
                var singleMesh = new Mesh();
                singleMesh.Append(tempMeshArray);
                singleMesh.Faces.ConvertQuadsToTriangles();
                meshOut[i]=(singleMesh);
                sizeList[i] = size;
                cenPtList[i] = cenPt;
            });

            //创建mesh simple，输出中心点与面积
            var simpleMesh = RhinoToolManager.ConvertFromRhMesh(meshOut);
            #endregion

            #region 输出内容
            DA.SetData(0, simpleMesh);
            DA.SetDataList(1, sizeList);
            DA.SetDataList(2, cenPtList);

            #endregion
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Resources.Urban_Sustainability_Energy;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("B5325E21-D79E-400C-93C3-9F74519CC5EA"); }
        }
    }
}
