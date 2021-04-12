using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rh=Rhino.Geometry;

using UrbanX.Planning.IndexCalc;
using UrbanX.Planning.UrbanDesign;

using UrbanX_GH.Properties;
using UrbanX_GH.Application;
using UrbanX_GH.Application.Geometry;


// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace UrbanX_GH
{
    public class UrbanX_Sustainability_ExposureRate3DComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public XElement meta;
        public static string c_id = "Urban_Sustainability_ExposureRate3D";
        public static string c_moduleName = "Urban_Sustainability";

        #region 备用
        //public Urban_SustainabilityComponent()
        //  : base("IndexCalculation", "IndexCalc",
        //      "index calculation, included EC,WC, GC, Population Amount",
        //      "UrbanXFireFly", "AutoGenerator")
        //{
        //}
        #endregion
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public UrbanX_Sustainability_ExposureRate3DComponent() : base("", "", "", "", "")
        {
            //ToDo 完善这部分内容
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
            //ToDo 完善这部分内容
            this.meta = SharedResources_Utils.GetXML(c_moduleName, c_id);
            List<XElement> list = this.meta.Element("inputs").Elements("input").ToList<XElement>();
            pManager.AddGenericParameter((string)list[0].Attribute("name"), (string)list[0].Attribute("nickname"), (string)list[0].Attribute("description"), GH_ParamAccess.item);
            pManager.AddGenericParameter((string)list[0].Attribute("name"), (string)list[0].Attribute("nickname"), (string)list[0].Attribute("description"), GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //ToDo 完善这部分内容
            this.meta = SharedResources_Utils.GetXML(c_moduleName, c_id);
            List<XElement> list = this.meta.Element("outputs").Elements("output").ToList<XElement>();
            pManager.AddGenericParameter((string)list[0].Attribute("name"), (string)list[0].Attribute("nickname"), (string)list[0].Attribute("description"), GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Rh.Mesh[] meshInput = null;
            double gridSize = 10;
            if (!DA.GetData(0, ref meshInput)) { return; }
            if (!DA.GetData(1, ref gridSize)) { return; }

            #region 细分mesh
            var jsonFilePath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\building_center.geojson";
            //var jsonFilePath = @"C:\Users\CAUPD-BJ141\Desktop\西安建筑基底_32650.geojson";
            string exportPath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\export_collection_center.obj";

            //创建mesh simple，输出中心点与面积
            var simpleMesh = MeshCreation.ExtrudeMeshFromPtMinusTopBtn(inputDataCollection, heightCollection, out Dictionary<NetTopologySuite.Geometries.Point, double> secPtDic);

            //初始化可视点数据
            var ptOrigin = new List<NetTopologySuite.Geometries.Point>() {
                new  NetTopologySuite.Geometries.Point(0,0),
                new  NetTopologySuite.Geometries.Point(-100,-100),
                new  NetTopologySuite.Geometries.Point(-200,-200),
                new  NetTopologySuite.Geometries.Point(-300,-300),
                new  NetTopologySuite.Geometries.Point(-400,-400),
            };
            var count = 1;
            var ptLargeList = new List<NetTopologySuite.Geometries.Point>(count * ptOrigin.Count);

            for (int i = 0; i < count; i++)
                ptLargeList.AddRange(ptOrigin);

            //开始计算可视范围内的点，及所包含建筑的总面积（去除顶面和底面）
            var wholePtList = Poly2DCreation.ContainsInPts(ptLargeList.ToArray(), secPtDic.Keys.ToArray(), 300);
            var wholeAreaList = Poly2DCreation.ContainsAreaInPts(wholePtList, secPtDic);

            //创建细分mesh
            var remeshedMesh = MeshCreation.SimpleRemesher(simpleMesh, 10, 0.5);

            //计算射线及比例，按照largeList顺序
            var visibleAreaList = MeshCreation.CalcRaysGetArea(remeshedMesh, MeshCreation.NTSPtList2Vector3dList(ptLargeList), 10, 100, 360, 200, 14);
            var visibilityPercentage = MeshCreation.CalcVisibilityPercent(visibleAreaList, wholeAreaList);

            #endregion

            #region 输出内容
            DA.SetDataList(0, brep_mesh);

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
            get { return new Guid("32108E14-0823-46ED-9C3B-934AEE76C5EB"); }
        }
    }
}
