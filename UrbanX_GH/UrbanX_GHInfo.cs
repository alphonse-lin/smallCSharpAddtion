using System;
using System.Drawing;
using Grasshopper.Kernel;

//using SharedResources;
//using SharedResources.Properties;
using UrbanX_GH.Properties;

namespace UrbanX_GH
{
    public class UrbanX_GHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "UrbanX_GH";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Resources.iconForAll;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("c9a01a83-ff9e-4959-8539-806e8c5da126");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
