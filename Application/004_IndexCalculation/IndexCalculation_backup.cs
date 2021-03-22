//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UrbanXX.IO.GeoJSON;

//namespace UrbanX.Application
//{
//    public enum QualityType
//    {
//        highQuality,
//        lowQuality
//    }
//    public class IndexCalculation
//    {
//        private const int roundNum = 1;
//        private string connectionString { get; set; }
//        private string city;
//        private string jsonFilePath;
//        private Dictionary<string, InfoFromXML_BB> Con_Buildings { get; set; }
//        private Dictionary<string, InfoFromXML_BB> Con_Blocks { get; set; }
//        private Dictionary<int, InfoFromXML_Population> PopulationType { get; set; }

//        private Dictionary<string, string> FunctionName = new Dictionary<string, string> {
//            { "R", "residential"},
//            { "H", "hotel"},
//            { "O", "office"},
//            { "C", "shopping_center"},
//            { "M", "industrial"},
//            { "W", "warehouse"},
//        };
//        public IndexCalculation(string xmlPath)
//        {
//            Con_Buildings = ReadXML_BB(xmlPath, "Buildings");
//            Con_Blocks = ReadXML_BB(xmlPath, "Blocks");
//            PopulationType = ReadXML_PP(xmlPath);
//        }

//        #region 功能区

//        #region 001_人口计算
//        private int SpecifyPopulationType(int layer)
//        {
//            int type = -1;
//            var ppList = PopulationType.Values.ToList();
//            List<double[]> intervalLayer = new List<double[]>();
//            for (int i = 0; i < ppList.Count; i++)
//                intervalLayer.Add(ppList[i]._layer);
//            for (int i = 0; i < ppList.Count; i++)
//            {
//                if (layer >= intervalLayer[i][0] && layer <= intervalLayer[i][1]) { type = i; break; }
//                else if (i == 4 && layer > intervalLayer[i][1])
//                    type = 4;
//                else
//                    continue;
//            }
//            return type;
//        }

//        public double[] CalculatePopulation(int[] layer, double[] area)
//        {
//            var len = layer.Count();
//            double[] result = new double[layer.Count()];
//            for (int i = 0; i < len; i++)
//            {
//                var type = SpecifyPopulationType(layer[i]);
//                var peopleUnitValue = PopulationType[type]._people;
//                result[i] = area[i] / peopleUnitValue;
//            }
//            return result;
//        }

//        public double CalculatePopulation(int layer, double area)
//        {
//            var type = SpecifyPopulationType(layer);
//            var peopleUnitValue = PopulationType[type]._people;
//            var result = area / peopleUnitValue;
//            return result;
//        }
//        #endregion

//        #region 读取XML

//        /// <summary>
//        /// 读取XML文件
//        /// </summary>
//        /// <param name="path">输入 xml 路径</param>
//        /// <param name="level">输入 Buildings 或者 Blocks</param>
//        /// <returns></returns>
//        private Dictionary<string, InfoFromXML_BB> ReadXML_BB(string path, string level)
//        {
//            Dictionary<string, InfoFromXML_BB> DicXML = InfoFromXML_BB.CreateDicFromXML(path, level);
//            return DicXML;
//        }
//        private Dictionary<int, InfoFromXML_Population> ReadXML_PP(string path)
//        {
//            Dictionary<int, InfoFromXML_Population> DicXML = InfoFromXML_Population.CreateDicFromXML(path);
//            return DicXML;
//        }
//        #endregion

//        #region 地块向指标计算

//        private double FAR_Block(double FAR)
//        {
//            double FARBLK = FAR;
//            return FARBLK;
//        }

//        private double Density_Block(double density)
//        {
//            double DensityBLK = density;
//            return DensityBLK;
//        }

//        private int[] Consumption_Block(string function, IEnumerable<int[]> Consumption_Building)
//        {
//            int[] Consumption = new int[3];
//            var length = Consumption_Building.Count();
//            int[] minGC = new int[length];
//            int[] maxGC = new int[length];
//            for (int i = 0; i < length; i++)
//            {
//                minGC[i] = Convert.ToInt32(Consumption_Building.ElementAt(i)[0]);
//                maxGC[i] = Convert.ToInt32(Consumption_Building.ElementAt(i)[1]);
//            }
//            Consumption[0] = Sum(minGC);
//            Consumption[1] = Sum(maxGC);
//            Consumption[2] = (Consumption[0] + Consumption[1]) / 2;

//            return Consumption;
//        }
//        #endregion

//        #region 建筑向指标计算
//        private int Layers_Brep(int layer)
//        {
//            int layerBD = layer;
//            return layerBD;
//        }
//        private string Function_Brep(string function)
//        {
//            string functionBD = function;
//            return functionBD;
//        }

//        public int[] EnergyConsumption_Brep(string function, double area)
//        {
//            int[] EConsumption = new int[3];
//            //单位：千瓦/平方米
//            double[] EConNum = Con_Buildings[function]._EConsumption;

//            EConsumption[0] = Convert.ToInt32(area * EConNum[0]);
//            EConsumption[1] = Convert.ToInt32(area * EConNum[1]);
//            EConsumption[2] = (EConsumption[0] + EConsumption[1]) / 2;

//            return EConsumption;
//        }

//        public int[] WaterConsumption_Brep(string function, double area)
//        {
//            int[] WConsumption = new int[3];
//            //单位：吨/平方米
//            double[] WConNum = Con_Buildings[function]._WConsumption;

//            WConsumption[0] = Convert.ToInt32(area * WConNum[0]);
//            WConsumption[1] = Convert.ToInt32(area * WConNum[1]);
//            WConsumption[2] = (WConsumption[0] + WConsumption[1]) / 2;

//            return WConsumption;
//        }

//        public int[] GarbageConsumption_Brep(string function, double area, int layer)
//        {
//            int[] GConsumption = new int[3];
//            var populationCount = CalculatePopulation(layer, area);
//            //单位：千克/人
//            var GConNum = Con_Buildings[function]._GConsumption;

//            GConsumption[0] = Convert.ToInt32(populationCount * GConNum[0]);
//            GConsumption[1] = Convert.ToInt32(populationCount * GConNum[1]);
//            GConsumption[2] = (GConsumption[0] + GConsumption[1]) / 2;

//            return GConsumption;
//        }

//        public int[] Consumption_BrepSum(List<int[]> IList)
//        {
//            int[] GConsumption = new int[2];
//            var len = IList.Count();
//            var minList = new List<int>(len);
//            var maxList = new List<int>(len);
//            for (int i = 0; i < len; i++)
//            {
//                int[] valueCount = IList[i];
//                minList.Add(valueCount[0]);
//                maxList.Add(valueCount[1]);
//            }
//            var minResult = minList.Sum();
//            var maxResult = maxList.Sum();

//            GConsumption[0] = minResult;
//            GConsumption[1] = maxResult;

//            return GConsumption;
//        }
//        public double GetAverageValue(double[] arrayValue)
//        {
//            var average = (arrayValue[0] + arrayValue[1]) / 2;
//            return average;
//        }

//        #endregion

//        #region 成本计算
//        public CalcConstructionCost ConstructionCost_Brep(string funcName, int floor, double area, Dictionary<string, SortedList<int, Quality>> costPrice, QualityType qType)
//        {
//            var houseLevel = 10;
//            Quality highQuality = new Quality();
//            Quality lowQuality = new Quality();
//            var min = 0;
//            var max = 0;
//            var average = 0;

//            switch (funcName)
//            {
//                case "R":
//                    if (floor < houseLevel)
//                    {
//                        highQuality = costPrice[string.Format("{0}_house", FunctionName[funcName])][0];
//                        lowQuality = costPrice[string.Format("{0}_house", FunctionName[funcName])][1];
//                    }
//                    else
//                    {
//                        highQuality = costPrice[string.Format("{0}_highRise", FunctionName[funcName])][0];
//                        lowQuality = costPrice[string.Format("{0}_highRise", FunctionName[funcName])][2];
//                    }
//                    break;
//                case "H":
//                    highQuality = costPrice[FunctionName[funcName]][0];
//                    lowQuality = costPrice[FunctionName[funcName]][1];
//                    break;
//                case "O":
//                    highQuality = costPrice[FunctionName[funcName]][0];
//                    lowQuality = costPrice[FunctionName[funcName]][2];
//                    break;
//                case "C":
//                    highQuality = costPrice[FunctionName[funcName]][0];
//                    lowQuality = costPrice[FunctionName[funcName]][1];
//                    break;
//                case "M":
//                    highQuality = costPrice[FunctionName[funcName]][0];
//                    lowQuality = costPrice[FunctionName[funcName]][1];
//                    break;
//                case "W":
//                    highQuality = costPrice[FunctionName[funcName]][0];
//                    lowQuality = costPrice[FunctionName[funcName]][1];
//                    break;
//            }

//            switch (qType)
//            {
//                case QualityType.highQuality:
//                    min = Convert.ToInt32(highQuality.priceMin * area);
//                    max = Convert.ToInt32(highQuality.priceMax * area);
//                    average = (min + max) / 2;
//                    break;
//                case QualityType.lowQuality:
//                    min = Convert.ToInt32(lowQuality.priceMin * area);
//                    max = Convert.ToInt32(lowQuality.priceMax * area);
//                    average = (min + max) / 2;
//                    break;
//            }
//            return new CalcConstructionCost(funcName, min, max, average);
//        }

//        public Dictionary<string, int[]> ConstructionCost_SUM(List<CalcConstructionCost> ccList)
//        {
//            Dictionary<string, List<int[]>> temp_ccList = new Dictionary<string, List<int[]>>();
//            List<int> minCostList = new List<int>(ccList.Count);
//            List<int> averageCostList = new List<int>(ccList.Count);
//            List<int> maxCostList = new List<int>(ccList.Count);

//            for (int i = 0; i < ccList.Count; i++)
//            {
//                int[] price = new int[3];
//                price[0] = ccList[i].CostMin;
//                price[1] = ccList[i].CostAverage;
//                price[2] = ccList[i].CostMax;
//                if (temp_ccList.ContainsKey(ccList[i].Name))
//                {
//                    temp_ccList[ccList[i].Name].Add(price);
//                }
//                else
//                {
//                    temp_ccList.Add(ccList[i].Name, new List<int[]> { price });
//                }
//            }

//            for (int i = 0; i < temp_ccList.Keys.Count; i++)
//            {
//                var keyName = temp_ccList.Keys.ElementAt(i);
//                for (int j = 0; j < temp_ccList[keyName].Count; j++)
//                {
//                    minCostList.Add(temp_ccList[keyName][j][0]);
//                    averageCostList.Add(temp_ccList[keyName][j][1]);
//                    maxCostList.Add(temp_ccList[keyName][j][2]);
//                }
//            }

//            Dictionary<string, int[]> result = new Dictionary<string, int[]>();
//            for (int i = 0; i < temp_ccList.Keys.Count; i++)
//            {
//                result.Add(temp_ccList.Keys.ElementAt(i), new int[] { minCostList.Sum(), averageCostList.Sum(), maxCostList.Sum() });
//            }
//            return result;
//        }

//        private Dictionary<string, SortedList<int, Quality>> ExtractCostData(string connectionString, string city)
//        {
//            int attrCount = 9;
//            string sql = string.Format("select " +
//                "bf.name,qf.quality_name,cc.quality_id,c.name, cc.price_max, cc.price_min,cc.year,c.lat, c.lon " +
//                "FROM construction_cost cc, cities c, building_functions bf, quality_functions qf " +
//                "where cc.city_id = c.code and cc.func_id = bf.id and cc.quality_id = qf.quality_id and c.name='{0}'; ", city);

//            var resultList = DB_Manager.GetData(connectionString, sql, attrCount);
//            FuncionClass cityInfo = new FuncionClass(resultList);
//            Dictionary<string, SortedList<int, Quality>> result = cityInfo.FuncInfoDic;

//            return result;
//        }
//        #endregion

//        #region 基础运算
//        private double Sum(IEnumerable<double> num)
//        {
//            double sum = 0d;
//            foreach (var item in num) { sum += item; }
//            return sum;
//        }

//        private int Sum(IEnumerable<int> num)
//        {
//            int sum = 0;
//            foreach (var item in num) { sum += item; }
//            return sum;
//        }
//        #endregion
//        #endregion
//    }

//    public class CalcConstructionCost
//    {
//        public string Name { get; set; }
//        public int CostMin { get; set; }
//        public int CostMax { get; set; }
//        public int CostAverage { get; set; }

//        public CalcConstructionCost(string name, int costMin, int costMax, int costAverage)
//        {
//            Name = name;
//            CostMin = costMin;
//            CostMax = costMax;
//            CostAverage = costAverage;
//        }
//    }
//}
