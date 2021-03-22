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
using AnalysisSpatial_Exposure;

using UrbanX_GH.Properties;


// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace UrbanX_GH
{
    public class UrbanX_Sustainability_Custom_EnergyComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public XElement meta;
        public static string c_id = "Urban_Sustainability_Custom_Energy";
        public static string c_moduleName = "Urban_Sustainability";

        #region 备用
        //public Urban_SustainabilityComponent()
        //  : base("IndexCalculation", "IndexCalc",
        //      "index calculation, included EC,WC, GC, Population Amount",
        //      "UrbanXFireFly", "AutoGenerator")
        //{
        //}
        #endregion
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public UrbanX_Sustainability_Custom_EnergyComponent() : base("", "", "", "", "")
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
            pManager.AddGenericParameter((string)list[1].Attribute("name"), (string)list[1].Attribute("nickname"), (string)list[1].Attribute("description"), GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            this.meta = SharedResources_Utils.GetXML(c_moduleName, c_id);
            List<XElement> list = this.meta.Element("outputs").Elements("output").ToList<XElement>();
            pManager.AddGenericParameter((string)list[0].Attribute("name"), (string)list[0].Attribute("nickname"), (string)list[0].Attribute("description"), GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //var xmlPath = @"D:\实验室\010_CAAD\002_插件\UrbanXFireFly\IndexCalc\bin\indexCalculation.xml";
            var defaultPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dataDirectory = Path.Combine(defaultPath, "data");
            string xmlPath = Path.Combine(dataDirectory, "indexCalculation.xml");
            var heightEachLayer = 3d;

            List<Brep> siteBreps = new List<Brep>();
            List<string> siteFunctions = new List<string>();
            if (!DA.GetDataList(0, siteBreps)) { return; }
            if (!DA.GetDataList(1, siteFunctions)) { return; }

            var indexCalc = new IndexCalculation(xmlPath);

            #region 层级数据输入
            //Block层
            DataTree<int> outputEC = new DataTree<int>();

            //read height
            for (int i = 0; i < siteBreps.Count; i++)
            {
                var ptList = siteBreps[i].Vertices;
                var tempMax = 0d;
                var tempMin = ptList[0].Location.Z;
                var faceBottomIndex = 0;

                for (int ptID = 0; ptID < ptList.Count; ptID++)
                {
                    tempMax = (ptList[ptID].Location.Z > tempMax) ? ptList[ptID].Location.Z : tempMax;
                    tempMin = (ptList[ptID].Location.Z < tempMin) ? ptList[ptID].Location.Z : tempMin;
                }
                //层数
                int layer = (int)Math.Ceiling((tempMax - tempMin) / heightEachLayer);

                //底面线
                for (int faceID = 0; faceID < siteBreps[i].Faces.Count; faceID++)
                {
                    var facePtZValue = siteBreps[i].Faces[faceID].PointAt(0.5, 0.5).Z;
                    if (facePtZValue == tempMin) { faceBottomIndex = faceID; break; }
                }
                var baseCrvArea = siteBreps[i].Faces[faceBottomIndex].ToBrep().GetArea();
                var tempECBuilding = indexCalc.EnergyConsumption_Building(siteFunctions[i], baseCrvArea * layer);

                GH_Path ghPath = new GH_Path(i);
                for (int j = 0; j < 2; j++)
                {
                    outputEC.Add(tempECBuilding[j], ghPath);
                }
            }
            #endregion

            #region 输出内容
            DA.SetDataTree(0, outputEC);

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
                return Resources.Urban_Sustainability_Custom_Energy;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("234E20F3-A417-4AF8-8E8C-12C7315B68FB"); }
        }
    }
}
