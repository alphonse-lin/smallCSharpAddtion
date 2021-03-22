using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UrbanX.Planning.IndexCalc
{
    public class InfoFromXML_Population
    {
        public double[] _layer { get; set; }
        public int _people { get; set; }
        public double[] _FAR { get; set; }
        public double _maxDensity { get; set; }
        public double _minGreen { get; set; }
        public double _maxHeight { get; set; }

        public InfoFromXML_Population(int populationType, string[] layer, int people, string[] FAR, double maxDensity, double minGreen, double maxHeight)
        {
            _layer = strArray2DouArray(layer);
            _people = people;
            _FAR = strArray2DouArray(FAR);
            _maxDensity = maxDensity;
            _minGreen = minGreen;
            _maxHeight = maxHeight;
        }

        #region 功能区

        public XElement indexCalc;
        public static string c_id = "Urban_Sustainability_Custom_Garbage";
        public static string c_moduleName = "Urban_Sustainability";


        //#endregion
        //public override GH_Exposure Exposure => GH_Exposure.secondary;
        //public UrbanX_Sustainability_Custom_GarbageComponent() : base("", "", "", "", "")
        //{
        //    //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(SharedUtils.Resolve);
        //    this.meta = SharedResources_Utils.GetXML(c_moduleName, c_id);
        //    this.Name = this.meta.Element("name").Value;
        //    this.NickName = this.meta.Element("nickname").Value;
        //    this.Description = this.meta.Element("description").Value + "\nv.1";
        //    this.Category = this.meta.Element("category").Value;
        //    this.SubCategory = this.meta.Element("subCategory").Value;
        //}



        public static Dictionary<int, InfoFromXML_Population> CreateDicFromXML(string xmlFileName, string level = "PopulationIndex")
        {
            Dictionary<int, InfoFromXML_Population> finalDic = new Dictionary<int, InfoFromXML_Population>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFileName);

            var nodeList = xmlDoc.SelectNodes($"//IndexCalculation/{level}/Population");

            int length = nodeList.Count;
            for (int i = 0; i < nodeList.Count; i++)
            {
                int populationType = int.Parse(nodeList[i].Attributes["Type"].Value);
                string[] Layer = nodeList[i]["Layer"].InnerText.Split(',');
                int People = int.Parse(nodeList[i]["People"].InnerText);
                string[] FAR = nodeList[i]["FAR"].InnerText.Split(',');
                double MaxDensity = double.Parse(nodeList[i]["MaxDensity"].InnerText);
                double MinGreen = double.Parse(nodeList[i]["MinGreen"].InnerText);
                double MaxHeight = double.Parse(nodeList[i]["MaxHeight"].InnerText);

                InfoFromXML_Population populationInfo = new InfoFromXML_Population(populationType, Layer,People,FAR,MaxDensity,MinGreen,MaxHeight);
                finalDic.Add(populationType, populationInfo);
            }
            return finalDic;
        }

        public double[] strArray2DouArray(string[] strArray)
        {
            double[] douArray = new double[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                douArray[i] = double.Parse(strArray[i]);
            }
            return douArray;
        }

        #endregion
    }
}
