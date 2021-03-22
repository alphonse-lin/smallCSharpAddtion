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

using UrbanX_GH.Properties;


// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace UrbanX_GH
{
    public class UrbanX_Sustainability_EnergyComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public XElement meta;
        public static string c_id = "Urban_Sustainability_Energy";
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
        public UrbanX_Sustainability_EnergyComponent() : base("", "", "", "", "")
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
            pManager.AddGenericParameter((string)list[0].Attribute("name"), (string)list[0].Attribute("nickname"), (string)list[0].Attribute("description"), GH_ParamAccess.item);
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

            DesignResult[] siteResults = null;
            if (!DA.GetData(0, ref siteResults)) { return; }

            var indexCalc = new IndexCalculation(xmlPath);

            #region 层级数据输入
            //Block层
            //DataTree<Brep> outputBrep = new DataTree<Brep>();
            DataTree<int> outputEC = new DataTree<int>();

            for (int blockID = 0; blockID < siteResults.Length; blockID++)
            {
                var siteResult = siteResults[blockID];

                //Subsite层
                for (int subSiteID = 0; subSiteID < siteResult.SubSites.Length; subSiteID++)
                {
                    //Building层
                    for (int buildingID = 0; buildingID < siteResults[blockID].SubSiteBuildingGeometries[subSiteID].Length; buildingID++)
                    {
                        var building = siteResults[blockID].SubSiteBuildingGeometries[subSiteID][buildingID];
                        //Brep层
                        for (int brepID = 0; brepID < building.BrepOutlines.Length; brepID++)
                        {
                            //Brep=数值归位
                            var tempECBuilding = indexCalc.EnergyConsumption_Building(building.BrepFunctions[brepID], building.BrepAreas[brepID]);

                            //传入GH Tree 
                            GH_Path ghPath = new GH_Path(blockID, subSiteID, buildingID, brepID);

                            //outputBrep.Add(building.Breps[brepID], ghPath);
                            for (int i = 0; i < 2; i++)
                            {
                                outputEC.Add(tempECBuilding[i], ghPath);
                            }
                        }
                    }
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
            get { return new Guid("80B597C9-5C26-4A35-B004-B54B93200C59"); }
        }
}
}
