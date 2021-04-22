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


// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace UrbanX_GH
{
    public class GH_Debug_Path_Component : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>

        #region 备用
        //public Urban_SustainabilityComponent()
        //  : base("IndexCalculation", "IndexCalc",
        //      "index calculation, included EC,WC, GC, Population Amount",
        //      "UrbanXFireFly", "AutoGenerator")
        //{
        //}
        #endregion
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public GH_Debug_Path_Component() : base("", "", "", "", "")
        {
            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(SharedUtils.Resolve);;
            this.Name = "GH_Debug_ExportPath";
            this.NickName = "GD_Path";
            this.Description = "this component is used for debuging, export the value of internal path";
            this.Category = "UrbanX";
            this.SubCategory = "GH_Debug";
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddBooleanParameter("initiate", "I", "toggle to start this component", GH_ParamAccess.item,false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("outputPath", "P", "export path", GH_ParamAccess.item);
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

            #region 输出内容
            DA.SetData(0, xmlPath);

            #endregion
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        //protected override System.Drawing.Bitmap Icon
        //{
        //    get
        //    {
        //        // You can add image files to your project resources and access them like this:
        //        //return Resources.IconForThisComponent;
        //        return null;
        //    }
        //}

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("178EB151-F0CB-45CE-AF14-89BEB7738199"); }
        }
    }
}
