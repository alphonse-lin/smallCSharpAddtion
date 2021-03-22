using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UrbanX.Planning.IndexCalc
{
    public class InfoFromXML_BB
    {
        public string _type;

        public double[] _EConsumption { get; set; }
        public double[] _WConsumption { get; set; }
        public double[] _GConsumption { get; set; }

        public InfoFromXML_BB(string type, string[] EConsumption, string[] WConsumption, string[] GConsumption)
        {
            _type = type;
            _EConsumption = strArray2DouArray(EConsumption);
            _WConsumption = strArray2DouArray(WConsumption);
            _GConsumption = strArray2DouArray(GConsumption);
        }


        #region 功能区
        public static Dictionary<string, InfoFromXML_BB> CreateDicFromXML(string xmlFileName, string level = "Buildings")
        {
            Dictionary<string, InfoFromXML_BB> finalDic = new Dictionary<string, InfoFromXML_BB>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFileName);

            var nodeList = xmlDoc.SelectNodes($"//IndexCalculation/{level}/{level.Remove(level.Length - 1, 1)}");
            for (int i = 0; i < nodeList.Count; i++)
            {
                string BdType = nodeList[i].Attributes["Type"].Value;
                string[] BdECon = nodeList[i]["EConsumption"].InnerText.Split(',');
                string[] BdWCon = nodeList[i]["WConsumption"].InnerText.Split(',');
                string[] BdGCon = nodeList[i]["GConsumption"].InnerText.Split(',');

                InfoFromXML_BB BdInfo = new InfoFromXML_BB(BdType, BdECon, BdWCon, BdGCon);
                finalDic.Add(BdType, BdInfo);
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
