using System;
using System.Linq;
using System.Drawing;
//using SharedResources.Properties;
using System.Xml.Linq;
using System.Reflection;
using System.IO;

using UrbanX_GH.Properties;

namespace UrbanX_GH
{
    public static class SharedResources_Utils
    {
        public static string TimerVersion = "v." + DateTime.Now.ToShortDateString();
        public static string AssemblyName = "UrbanX";
        public static Bitmap AssemblyIcon = Resources.iconForAll;
        public static string AssemblyDescription = "UrbanX GH plugin for urban auto generator";
        public static string AssemblyAuthor = "Luo, Lin, Deng, Yang";
        public static Guid AssemblyGuid = new Guid("C2B39A46-47F0-45BF-A933-727D5FE575E5");
        public static string AssemblyContacts = "CAUPD UrbanX Lab";


        public static XElement GetXML(string moduleName, string componentId)
        {
            return (from c in (from c in XDocument.Parse(Resources.MetaData).Root.Descendants("module")
                               where (string)c.Attribute("name") == moduleName
                               select c).Elements("component")
                    where c.Element("id").Value == componentId
                    select c).First();
        }

        public static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;
            if (name.Contains(".resources")) { return null; }
            string name2 = name + ".dll";
            Assembly.GetAssembly(typeof(SharedResources_Utils)).GetManifestResourceNames();
            Assembly result;
            using (Stream manifestResourceStream = Assembly.GetAssembly(typeof(SharedResources_Utils)).GetManifestResourceStream(name2))
            {
                if (manifestResourceStream == null) { result = null; }
                else
                {
                    byte[] array = new byte[manifestResourceStream.Length];
                    manifestResourceStream.Read(array, 0, array.Length);
                    result = Assembly.Load(array);
                }
            }
            return result;
        }
    }
}
