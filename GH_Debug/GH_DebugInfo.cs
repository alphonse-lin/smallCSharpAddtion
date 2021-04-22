using System;
using System.Drawing;
using Grasshopper.Kernel;

//using SharedResources;
//using SharedResources.Properties;

namespace UrbanX_GH
{
    public class GH_DebugInfo : GH_AssemblyInfo
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
                return null;
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
                return new Guid("C697BB9C - CB57 - 4B83 - 98FE - 7F171AC2F177");
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
