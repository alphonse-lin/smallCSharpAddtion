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
using g3;
using System.Collections.Concurrent;


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
        public static string c_id = "Urban_Spatial_Analysis_ExposureRate3D";
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
            pManager.AddPointParameter((string)list[0].Attribute("name"), (string)list[0].Attribute("nickname"), (string)list[0].Attribute("description"), GH_ParamAccess.list);
            pManager.AddGenericParameter((string)list[1].Attribute("name"), (string)list[1].Attribute("nickname"), (string)list[1].Attribute("description"), GH_ParamAccess.item);
            pManager.AddIntegerParameter((string)list[2].Attribute("name"), (string)list[2].Attribute("nickname"), (string)list[2].Attribute("description"), GH_ParamAccess.item,10);
            pManager.AddIntegerParameter((string)list[3].Attribute("name"), (string)list[3].Attribute("nickname"), (string)list[3].Attribute("description"), GH_ParamAccess.item,40);
            pManager.AddNumberParameter((string)list[4].Attribute("name"), (string)list[4].Attribute("nickname"), (string)list[4].Attribute("description"), GH_ParamAccess.item,200.0);
            pManager.AddNumberParameter((string)list[5].Attribute("name"), (string)list[5].Attribute("nickname"), (string)list[5].Attribute("description"), GH_ParamAccess.item,45.0);
            pManager[2].Optional = false;
            pManager[3].Optional = false;
            pManager[4].Optional = false;
            pManager[5].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //ToDo 完善这部分内容
            this.meta = SharedResources_Utils.GetXML(c_moduleName, c_id);
            List<XElement> list = this.meta.Element("outputs").Elements("output").ToList<XElement>();
            pManager.AddGenericParameter((string)list[0].Attribute("name"), (string)list[0].Attribute("nickname"), (string)list[0].Attribute("description"), GH_ParamAccess.item);
            pManager.AddGenericParameter((string)list[1].Attribute("name"), (string)list[1].Attribute("nickname"), (string)list[1].Attribute("description"), GH_ParamAccess.item);
            pManager.AddGenericParameter((string)list[2].Attribute("name"), (string)list[2].Attribute("nickname"), (string)list[2].Attribute("description"), GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Rh.Point3d> inputPtList = new List<Rh.Point3d>();
            GeneratedMeshClass inputGMClass = new GeneratedMeshClass();

            Rh.Mesh topBtnMesh = new Rh.Mesh();
            DMesh3 sidesMesh = new DMesh3();
            int segmentVertical = 10;
            int segmentHorizontal = 40;
            double viewRangeRadius = 200d;
            double viewAngleHeight = 45d;
            Colorf basedColor = Colorf.LightGrey;

            if (!DA.GetDataList(0, inputPtList)) { return; }
            if (!DA.GetData(1, ref inputGMClass)) { return; }
            
            if (!DA.GetData(2, ref segmentVertical)) { return; }
            if (!DA.GetData(3, ref segmentHorizontal)) { return; }
            if (!DA.GetData(4, ref viewRangeRadius)) { return; }
            if (!DA.GetData(5, ref viewAngleHeight)) { return; }

            topBtnMesh = inputGMClass.topBtnList;
            sidesMesh = inputGMClass.sideList;
            double[] inputAreaList = inputGMClass.sideAreaList;
            Rh.Point3d[] inputCentPtList = inputGMClass.cenPtList;

            ////创建mesh simple，输出中心点与面积
            //var simpleMesh=RhinoToolManager.ConvertFromRhMesh(inputMeshList);
            var secPtDic = MeshCreation.GenerateDic(inputAreaList,inputCentPtList);
            var ptLargeList = MeshCreation.ConvertFromRh_Point(inputPtList);

            //开始计算可视范围内的点，及所包含建筑的总面积（去除顶面和底面）
            var wholePtList = Poly2DCreation.ContainsInPts(ptLargeList, secPtDic.Keys.ToArray(), viewRangeRadius);
            var wholeAreaList = Poly2DCreation.ContainsAreaInPts(wholePtList, secPtDic);

            //计算射线及比例，按照largeList顺序
            //var rayResultDic = MeshCreation.CalcRays(loadedMesh, ptLargeList, 10, 100, 360, 200, 14);
            var ptLargeArray = ptLargeList.ToArray();
            ConcurrentDictionary<int, int> rayResultDic = new ConcurrentDictionary<int, int>();
            var visibleAreaList = new double[ptLargeArray.Length];
            var visibilityPercentage = new double[ptLargeArray.Length];

            var spatial=MeshCreation.InitialMeshTree(sidesMesh);
            
            System.Threading.Tasks.Parallel.For(0, ptLargeArray.Length, i =>
            {
                visibleAreaList[i] = MeshCreation.CalcRaysGetAreaParallel(sidesMesh, spatial, MeshCreation.NTSPt2Vector3d(ptLargeArray[i]), rayResultDic, segmentVertical, segmentHorizontal, 360, viewRangeRadius, viewAngleHeight);
                visibilityPercentage[i] = MeshCreation.CalcVisibilityPercentParallel(visibleAreaList[i], wholeAreaList[i]);
            });

            //输出内容
            ////初始化颜色
            MeshCreation.InitiateColor(sidesMesh, basedColor);
            var meshFromRays = MeshCreation.ApplyColorsBasedOnRays(sidesMesh, rayResultDic,Colorf.Yellow, Colorf.Red);

            //输出计算后Mesh
            //var exportPath_Calc = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\097_Geometry3D\测试\export_test.obj";
            //MeshCreation.ExportMeshAsObj(exportPath_Calc, meshFromRays, true);
            var rhSidesMesh =MeshCreation.ConvertFromDMesh3(meshFromRays);

            DA.SetData(0, rhSidesMesh);
            DA.SetData(1, topBtnMesh);
            DA.SetDataList(2, visibilityPercentage.ToList());
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
            get { return new Guid("E6ED1CAB-DEFE-47D0-9A14-6B5426E1980A"); }
        }
    }
}
