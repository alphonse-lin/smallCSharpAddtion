using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace utility
{
    public class XMLManager
    {
        static XMLManager() { }

        #region Xml to data
        #region CityConstructionCost_dataStructure
        //[0]       Type of Building
        //[1]       Office
        //[1][0]        High Quality
        //[1][1]        Medium Quality
        //[1][2]        Ordinary Quality
        //[2]       Shopping Centre
        //[2][0]        High Quality
        //[2][1]        Medium Quality
        //[3]       Residential_HighRise
        //[3][0]        High Rise; High Quality
        //[3][1]        High Rise; Medium Quality
        //[3][2]        High Rise; Ordinary Quality
        //[4]       Residential_House
        //[4][0]        House; High Quality
        //[4][1]        House; Medium Quality
        //[4][2]        Clubhouse
        //[4][3]        External works & landscaping(cost/m2 external area)
        //[5]       Hotel(including FF&E)
        //[5][0]        5-Star
        //[5][1]        3-Star
        //[6]       Industrial
        //[6][0]        Landlord; High Rise
        //[6][1]        End User; Low Rise
        //[7]       Carpark
        //[7][0]        Basement; up to 2 Levels
        //[7][1]        Multi-Storey
        #endregion
        public static List<ConstructionCostClass> xmlParseCities(string xmlPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlNode xn = xmlDoc.SelectSingleNode("Cities");
            XmlNodeList xnls = xn.ChildNodes;

            var citiesCostModelList = new List<ConstructionCostClass>();

            for (int i = 0; i < xnls.Count; i++)
            {
                var cityCostModel = new ConstructionCostClass();

                XmlElement xe_1 = (XmlElement)xnls[i];
                XmlNodeList xnl_1 = xe_1.ChildNodes;//type of building-office-hotel level

                cityCostModel.CityName = xnl_1.Item(0).InnerText;
                cityCostModel.CityID = xe_1.GetAttribute("Id").ToString();

                cityCostModel.Office = XmlNodeParse(xnl_1, 1);
                cityCostModel.Shopping = XmlNodeParse(xnl_1, 2);
                cityCostModel.Residential_HighRise = XmlNodeParse(xnl_1, 3);
                cityCostModel.Residential_House = XmlNodeParse(xnl_1, 4);
                cityCostModel.Hotel = XmlNodeParse(xnl_1, 5);
                cityCostModel.Industrial = XmlNodeParse(xnl_1, 6);
                cityCostModel.Carpark = XmlNodeParse(xnl_1, 7);
                
                citiesCostModelList.Add(cityCostModel);
            }

            return citiesCostModelList;
        }

        private static string[][] XmlNodeParse(XmlNodeList xnl_1, int index)
        {
            XmlNodeList xnl_2 = xnl_1.Item(index).ChildNodes;//high qulity-medium-ordinary level
            string[][] value = new string[xnl_2.Count][];
            for (int k = 0; k < xnl_2.Count; k++)
            {
                if (xnl_2.Item(k).InnerText.Contains("N/A"))
                {
                    value[k] = new string[2];
                    for (int i = 0; i < 2; i++)
                    {
                        value[k][i] = "N/A";
                    }
                }
                else
                {
                    value[k] = cleanString(xnl_2.Item(k).InnerText.Split(','));
                }
            }
            return value;
        }

        private static string[][][] XmlNodeParse_2(XmlNodeList xnl_1, int index)
        {
            XmlNodeList xnl_2 = xnl_1.Item(index).ChildNodes;//high qulity-medium-ordinary level
            string[][][] value = new string[xnl_2.Count][][];
            for (int i = 0; i < xnl_2.Count; i++)
            {
                XmlNodeList xnl_3 = xnl_2.Item(i).ChildNodes;
                for (int j = 0; j < xnl_3.Count; j++)
                {
                    value[i][j] = xnl_3.Item(i).InnerText.Split(',');
                }
            }
            return value;
        }

        private static string cleanString(string newStr)
        {
            if (newStr.Contains("\n")|| newStr.Contains("\r"))
            {
                var tempStr = newStr.Replace("\n", "");
                return tempStr.Replace("\r", "");
            }
            return newStr;
        }
        private static string[] cleanString(string[] newStr)
        {
            var resultString = new string[newStr.Length];
            for (int i = 0; i < newStr.Length; i++)
            {
                resultString[i]=cleanString(newStr[i]);
            }
            return resultString;
        }
        #endregion

        #region Excel to Xml
        public static void Excel2Xml(string excelFilePath, string exportXmlPath, string sheetTitle)
        {
            DataSet excelTableDataSet = new DataSet();
            string stringConnectToExcelFile = string.Format("provider=Microsoft.Jet.OLEDB.4.0;data source={0};Extended Properties=Excel 8.0;", excelFilePath);
            OleDbConnection oleConnectionToExcelFile = new OleDbConnection(stringConnectToExcelFile);
            OleDbDataAdapter oleDataAdapterForGetExcelTable = new OleDbDataAdapter(
                string.Format($"select * from [{sheetTitle}$]"), oleConnectionToExcelFile);
            try
            {
                oleDataAdapterForGetExcelTable.Fill(excelTableDataSet, "city");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("error:" + ex.Message);
            }

            CreateNewXElement(excelTableDataSet, exportXmlPath);
        }

        private static void CreateNewXElement(DataSet execelDataset, string testPath)
        {
            var xElement = new List<XElement>();//city
            var valueXElement = new List<XElement>();//value

            var rowTotalCount = execelDataset.Tables[0].Rows.Count;
            var columnData = execelDataset.Tables[0].Columns;
            var columnList = new List<string>();


            for (int rowId = 0; rowId < rowTotalCount; rowId++)
            {
                var idRecord = 0;
                var rowList = execelDataset.Tables[0].Rows[rowId].ItemArray.ToList();
                var typeXElement = new List<XElement>();
                for (int columnId = 0; columnId < columnData.Count; columnId++)
                {
                    columnList.Add(columnData[columnId].ColumnName.Replace(" ", ""));
                    if (rowList[columnId].ToString().Length == 0)
                    {
                        XElement singleTypeXElement = new XElement(columnList[idRecord], valueXElement); //type->office
                        typeXElement.Add(singleTypeXElement);

                        valueXElement = new List<XElement>();
                        idRecord = columnId;
                        //continue;
                    }
                    else
                    {
                        var tempName = columnList[columnId];
                        var tempValue = rowList[columnId];
                        var name = tempName;
                        if (tempName.Contains("$")) { name = tempName.Replace("Sheet2$.", ""); }
                        var value = tempValue.ToString().Replace(",", "");
                        var singleXElement = new XElement(name, (value.Replace(" - ", ",")));
                        valueXElement.Add(singleXElement);//value

                        if (columnId == columnData.Count - 1)
                        {
                            XElement singleTypeXElement = new XElement(columnList[idRecord], valueXElement); //type->office
                            typeXElement.Add(singleTypeXElement);
                            valueXElement = new List<XElement>();
                            idRecord = columnId;
                        }
                    }
                }
                var singleCity = new XElement("City", new XAttribute("Id", rowId), typeXElement);
                xElement.Add(singleCity);
            }
            var doc = new XDocument(new XElement("Cities", xElement));
            doc.Save(testPath);
        }
        #endregion
    }
}
