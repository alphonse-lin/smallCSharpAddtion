using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanXX.IO.GeoJSON;

namespace UrbanX.Application
{
    public enum QualityType
    {
        highQuality,
        lowQuality
    }
    public class Sustainable_calculation
    {
        private const int roundNum = 1;
        private string connectionString { get; set; }
        private string city;

        private List<double[]> pp_intervalLayer;
        private readonly Dictionary<string, string> FunctionName = new Dictionary<string, string> {
            { "R", "residential"},
            { "H", "hotel"},
            { "O", "office"},
            { "C", "shopping_center"},
            { "M", "industrial"},
            { "W", "warehouse"},
        };
        private Dictionary<string, List<BasePopulation>> PopulationValueDic { get; set; }
        private Dictionary<string, List<BaseEWGConsumption>> EWGConsumptionValueDic { get; set; }
        private Dictionary<string, List<Quality>> ConstructionValueDic { get; set; }

        public List<int> qTypeList;
        public ResultFromAll Result { get; set; }
        public Sustainable_calculation(string ConnectionString, string City)
        {
            connectionString = ConnectionString;
            city = City;

            Start();
        }

        public Sustainable_calculation(string ConnectionString, string City, IEnumerable<string> funcs, IEnumerable<int> layers, IEnumerable<double> areas, double highQualityRatio)
        {
            connectionString = ConnectionString;
            city = City;

            Start();
            Result=CalculateAll(funcs, layers, areas, highQualityRatio);
        }

        public Sustainable_calculation(string ConnectionString, string City, string filePath, double highQualityRatio)
        {
            connectionString = ConnectionString;
            city = City;

            Start();
            ExtractDataFromJSON readFromJSON = new ExtractDataFromJSON(filePath);
            Result = CalculateAll(readFromJSON.funcList, readFromJSON.layerList, readFromJSON.areaList, highQualityRatio);
        }

        #region 初始化
        private void Start()
        {
            //人口
            PopulationValueDic = ExtractData_Population();
            pp_intervalLayer = GenerateLayerCollection(PopulationValueDic);

            //EWG
            EWGConsumptionValueDic = ExtractData_EWGConsumption();

            //成本
            ConstructionValueDic = ExtractData_ConstructionCost();
        }
        private ResultFromAll CalculateAll(IEnumerable<string> funcArray, IEnumerable<int> layer, IEnumerable<double> area, double highQualityRatio=0.5)
        {
            var length = funcArray.Count();
            qTypeList = new List<int>(length);
            for (int i = 0; i < length; i++) { qTypeList.Add(ToolManagers.GenerateRandomInt(highQualityRatio)); }

            List<double> PopulationList = new List<double>(length);
            List<int[]> ECList = new List<int[]>(length);
            List<int[]> WCList = new List<int[]>(length);
            List<int[]> GCList = new List<int[]>(length);
            List<double[]> CCList_Int = new List<double[]>(length);
            List<ResultFromConstructionCost> CCList = new List<ResultFromConstructionCost>(length);

            for (int i = 0; i < length; i++)
            {
                PopulationList.Add(CalculatePopulation_Brep(funcArray.ElementAt(i), area.ElementAt(i), layer.ElementAt(i)));
                ECList.Add(EnergyConsumption_Brep(funcArray.ElementAt(i), area.ElementAt(i), layer.ElementAt(i)));
                WCList.Add(WaterConsumption_Brep(funcArray.ElementAt(i), area.ElementAt(i), layer.ElementAt(i)));
                GCList.Add(GarbageConsumption_Brep(funcArray.ElementAt(i), area.ElementAt(i), layer.ElementAt(i)));
                CCList.Add(ConstructionCost_Brep(funcArray.ElementAt(i), area.ElementAt(i), layer.ElementAt(i), ConstructionValueDic,(QualityType)qTypeList[i], out double[] intArray_result));
                CCList_Int.Add(intArray_result);
            }

            return new ResultFromAll(PopulationList,ECList,WCList,GCList, CCList,CCList_Int);
        }

        #endregion

        #region 功能区

        #region 001_人口计算
        /// <summary>
        /// 提取所有层高数据
        /// </summary>
        private List<double[]> GenerateLayerCollection(Dictionary<string, List<BasePopulation>> pv_Dic, string function = "R")
        {
            var ppLayerList = new List<double[]>(pv_Dic[function].Count);

            for (int i = 0; i < pv_Dic[function].Count; i++)
            {
                var tempData = new double[] { pv_Dic[function][i].layerMin, pv_Dic[function][i].layerMax };
                ppLayerList.Add(tempData);
            }
            return ppLayerList;
        }

        /// <summary>
        /// 确定属于建筑属于哪一类型
        /// </summary>
        private int SpecifyPopulationType(int layer)
        {
            int type = -1;
            for (int i = 0; i < pp_intervalLayer.Count; i++)
            {
                if (layer >= pp_intervalLayer[i][0] && layer <= pp_intervalLayer[i][1]) { type = i; break; }
                else if (i == 4 && layer > pp_intervalLayer[i][1])
                    type = 4;
                else
                    continue;
            }
            return type;
        }

        /// <summary>
        /// 基于面积和层高，计算人口数
        /// </summary>
        private double[] CalculatePopulation(int[] layer, double[] area, string function = "R")
        {
            var len = layer.Count();
            double[] result = new double[layer.Count()];
            for (int i = 0; i < len; i++)
            {
                var type = SpecifyPopulationType(layer[i]);
                var peopleUnitValue = PopulationValueDic[function][type].people;
                result[i] = area[i] / peopleUnitValue;
            }
            return result;
        }

        /// <summary>
        /// 基于面积和层高，计算人口数
        /// </summary>
        private double CalculatePopulation(int layer, double area, string function = "R")
        {
            var type = SpecifyPopulationType(layer);
            var peopleUnitValue = PopulationValueDic[function][type].people;
            var result = area / peopleUnitValue;
            return result;
        }

        /// <summary>
        /// 基于面积和层高，计算人口数
        /// </summary>
        public double CalculatePopulation_Brep(string function, double area, int layer)
        {
            var result = 0d;
            if (function == "R")
            {
                var type = SpecifyPopulationType(layer);
                var peopleUnitValue = PopulationValueDic[function][type].people;
                result = area / peopleUnitValue;
            }
            return result;
        }
        #endregion

        #region 002_地块向指标计算
        private double FAR_Block(double FAR)
        {
            double FARBLK = FAR;
            return FARBLK;
        }

        private double Density_Block(double density)
        {
            double DensityBLK = density;
            return DensityBLK;
        }

        private int[] Consumption_Block(string function, IEnumerable<int[]> Consumption_Building)
        {
            int[] Consumption = new int[3];
            var length = Consumption_Building.Count();
            int[] minGC = new int[length];
            int[] maxGC = new int[length];
            for (int i = 0; i < length; i++)
            {
                minGC[i] = Convert.ToInt32(Consumption_Building.ElementAt(i)[0]);
                maxGC[i] = Convert.ToInt32(Consumption_Building.ElementAt(i)[1]);
            }
            Consumption[0] = Sum(minGC);
            Consumption[1] = Sum(maxGC);
            Consumption[2] = (Consumption[0] + Consumption[1]) / 2;

            return Consumption;
        }
        #endregion

        #region 003_建筑向指标计算
        public int[] EnergyConsumption_Brep(string function, double area, int layer)
        {
            int[] EConsumption = new int[3];
            //单位：千瓦/平方米
            double[] EConNum = new double[] { EWGConsumptionValueDic[function][0].c_min, EWGConsumptionValueDic[function][0].c_max };

            EConsumption[0] = Convert.ToInt32(layer * area * EConNum[0]);
            EConsumption[1] = Convert.ToInt32(layer * area * EConNum[1]);
            EConsumption[2] = (EConsumption[0] + EConsumption[1]) / 2;

            return EConsumption;
        }

        public int[] WaterConsumption_Brep(string function, double area, int layer)
        {
            int[] WConsumption = new int[3];
            //单位：吨/平方米
            double[] WConNum = new double[] { EWGConsumptionValueDic[function][1].c_min, EWGConsumptionValueDic[function][1].c_max };

            WConsumption[0] = Convert.ToInt32(layer * area * WConNum[0]);
            WConsumption[1] = Convert.ToInt32(layer * area * WConNum[1]);
            WConsumption[2] = (WConsumption[0] + WConsumption[1]) / 2;

            return WConsumption;
        }

        public int[] GarbageConsumption_Brep(string function, double area, int layer)
        {
            int[] GConsumption = new int[3];
            var populationCount = CalculatePopulation(layer, area);
            //单位：千克/人
            var GConNum = new double[] { EWGConsumptionValueDic[function][2].c_min, EWGConsumptionValueDic[function][2].c_max };

            GConsumption[0] = Convert.ToInt32(populationCount * GConNum[0]);
            GConsumption[1] = Convert.ToInt32(populationCount * GConNum[1]);
            GConsumption[2] = (GConsumption[0] + GConsumption[1]) / 2;

            return GConsumption;
        }
        #endregion

        #region 004_成本计算
        public ResultFromConstructionCost ConstructionCost_Brep(string funcName, double area, int floor, Dictionary<string, List<Quality>> costPrice, QualityType qType, out double[] result)
        {
            var houseLevel = 10;
            Quality highQuality;
            Quality lowQuality;
            var min = 0d;
            var max = 0d;
            var average = 0d;
            var totalArea = floor * area;

            switch (funcName)
            {
                case "R":
                    if (floor < houseLevel)
                    {
                        switch (qType)
                        {
                            case QualityType.highQuality:
                                highQuality = costPrice[string.Format("{0}_house", FunctionName[funcName])][0];
                                CalculateBasedQualityNormal(highQuality, totalArea, out min, out max, out average);
                                break;
                            case QualityType.lowQuality:
                                lowQuality = costPrice[string.Format("{0}_house", FunctionName[funcName])][1];
                                CalculateBasedQualityNormal(lowQuality, totalArea, out min, out max, out average);
                                break;
                        }
                    }
                    else
                    {
                        switch (qType)
                        {
                            case QualityType.highQuality:
                                highQuality = costPrice[string.Format("{0}_highRise", FunctionName[funcName])][0]; ;
                                CalculateBasedQualityNormal(highQuality, totalArea, out min, out max, out average);
                                break;
                            case QualityType.lowQuality:
                                lowQuality = costPrice[string.Format("{0}_highRise", FunctionName[funcName])][2];
                                CalculateBasedQualityNormal(lowQuality, totalArea, out min, out max, out average);
                                break;
                        }
                    }
                    break;
                case "H":
                    switch (qType)
                    {
                        case QualityType.highQuality:
                            highQuality = costPrice[FunctionName[funcName]][0];
                            CalculateBasedQualityNormal(highQuality, totalArea, out min, out max, out average);
                            break;
                        case QualityType.lowQuality:
                            lowQuality = costPrice[FunctionName[funcName]][1];
                            CalculateBasedQualityNormal(lowQuality, totalArea, out min, out max, out average);
                            break;
                    }
                    break;
                case "O":
                    switch (qType)
                    {
                        case QualityType.highQuality:
                            highQuality = costPrice[FunctionName[funcName]][0];
                            CalculateBasedQualityNormal(highQuality, totalArea, out min, out max, out average);
                            break;
                        case QualityType.lowQuality:
                            lowQuality = costPrice[FunctionName[funcName]][2];
                            CalculateBasedQualityNormal(lowQuality, totalArea, out min, out max, out average);
                            break;
                    }
                    break;
                case "C":
                    switch (qType)
                    {
                        case QualityType.highQuality:
                            highQuality = costPrice[FunctionName[funcName]][0];
                            CalculateBasedQualityNormal(highQuality, totalArea, out min, out max, out average);
                            break;
                        case QualityType.lowQuality:
                            lowQuality = costPrice[FunctionName[funcName]][1];
                            CalculateBasedQualityNormal(lowQuality, totalArea, out min, out max, out average);
                            break;
                    }
                    break;
                case "M":
                    switch (qType)
                    {
                        case QualityType.highQuality:
                            highQuality = costPrice[FunctionName[funcName]][0];
                            CalculateBasedQualityNormal(highQuality, totalArea, out min, out max, out average);
                            break;
                        case QualityType.lowQuality:
                            lowQuality = costPrice[FunctionName[funcName]][1];
                            CalculateBasedQualityNormal(lowQuality, totalArea, out min, out max, out average);
                            break;
                    }
                    break;
                case "W":
                    switch (qType)
                    {
                        case QualityType.highQuality:
                            highQuality = costPrice[FunctionName[funcName]][0];
                            CalculateBasedQualityNormal(highQuality, totalArea, out min, out max, out average);
                            break;
                        case QualityType.lowQuality:
                            lowQuality = costPrice[FunctionName[funcName]][1];
                            CalculateBasedQualityNormal(lowQuality, totalArea, out min, out max, out average);
                            break;
                    }
                    break;
            }

            result = new double[] { min, max, average };

            return new ResultFromConstructionCost(funcName, min, max, average);
        }
        private void CalculateBasedQualityNormal(Quality quality, double totalArea, out double min, out double max, out double average)
        {
            min = quality.priceMin * totalArea;
            max = quality.priceMax * totalArea;
            average = (min + max) / 2d;
        }
        private Dictionary<string, List<Quality>> ExtractCostData(string connectionString, string city)
        {
            int attrCount = 9;
            string sql = string.Format("select " +
                "bf.name,qf.quality_name,cc.quality_id,c.name, cc.price_max, cc.price_min,cc.year,c.lat, c.lon " +
                "FROM construction_cost cc, cities c, building_functions bf, quality_functions qf " +
                "where cc.city_id = c.code and cc.func_id = bf.id and cc.quality_id = qf.quality_id and c.name='{0}'; ", city);

            var resultList = DB_Manager.GetData(connectionString, sql, attrCount);
            FuncionClass cityInfo = new FuncionClass(resultList);
            Dictionary<string, List<Quality>> result = cityInfo.FuncInfoDic;

            return result;
        }
        #endregion

        #region 005_碳中和
        #endregion


        #region 098读取数据库数据

        #region 098_001_读取数据库数据 Population
        private Dictionary<string, List<BasePopulation>> ExtractData_Population()
        {
            int attrCount = 12;
            string sql = "select " +
                "bp.bp_id, bp.bp_name, bp.bp_layer_min,bp.bp_layer_max, bp.bp_people, bp.bp_far_min, bp.bp_far_max," +
                "bp.bp_density_max,bp.bp_green_min,bp.bp_height_max,bf.name, bf.relative_name " +
                "from " +
                "building_population bp, building_functions bf " +
                "where" +
                " bp.bp_func_id = bf.id;";

            var temp_List = DB_Manager.GetData(connectionString, sql, attrCount);

            List<BasePopulation> valueList = new List<BasePopulation>();
            Dictionary<string, List<BasePopulation>> valueDic = new Dictionary<string, List<BasePopulation>>();
            for (int i = 0; i < temp_List.Count; i++)
            {
                var tempArray = temp_List[i];
                var singleC = new BasePopulation(tempArray[1], double.Parse(tempArray[2]), double.Parse(tempArray[3]), int.Parse(tempArray[4]), double.Parse(tempArray[5]), double.Parse(tempArray[6]),
                    double.Parse(tempArray[7]), double.Parse(tempArray[8]), double.Parse(tempArray[9]), tempArray[10], tempArray[11]
                    );
                if (valueDic.ContainsKey(singleC.relativeName))
                {
                    valueDic[singleC.relativeName].Add(singleC);
                }
                else
                {
                    valueDic.Add(singleC.relativeName, new List<BasePopulation> { singleC });
                }
                valueList.Add(singleC);
            }

            return valueDic;
        }

        #endregion

        #region 098_002_读取数据库数据 EC/WC/GC
        private Dictionary<string, List<BaseEWGConsumption>> ExtractData_EWGConsumption()
        {
            int attrCount = 6;
            string sql = "select cv.cv_id, cv.cv_name, bf.name, bf.relative_name, cv.cv_max, cv.cv_min " +
                "from building_consumption_value cv, building_functions bf " +
                "where cv.building_type = bf.id;";

            var temp_List = DB_Manager.GetData(connectionString, sql, attrCount);

            var valueList = new List<BaseEWGConsumption>();
            var valueDic = new Dictionary<string, List<BaseEWGConsumption>>();
            for (int i = 0; i < temp_List.Count; i++)
            {
                var tempArray = temp_List[i];
                BaseEWGConsumption singleC = new BaseEWGConsumption(tempArray[0], tempArray[2], tempArray[3], double.Parse(tempArray[4]), double.Parse(tempArray[5]));
                if (valueDic.ContainsKey(singleC.relativeName))
                {
                    valueDic[singleC.relativeName].Add(singleC);
                }
                else
                {
                    valueDic.Add(singleC.relativeName, new List<BaseEWGConsumption>() { singleC });
                }
                valueList.Add(singleC);
            }

            return valueDic;
        }

        #endregion

        #region 098_003_读取数据库数据 成本库
        private Dictionary<string, List<Quality>> ExtractData_ConstructionCost()
        {
            int attrCount = 9;
            string sql = string.Format("select " +
                "bf.name,qf.quality_name,cc.quality_id,c.name, cc.price_max, cc.price_min,cc.year,c.lat, c.lon " +
                "FROM construction_cost cc, cities c, building_functions bf, quality_functions qf " +
                "where cc.city_id = c.code and cc.func_id = bf.id and cc.quality_id = qf.quality_id and c.name='{0}'; ", city);

            var resultList = DB_Manager.GetData(connectionString, sql, attrCount);
            FuncionClass cityInfo = new FuncionClass(resultList);
            var result = cityInfo.FuncInfoDic;

            return result;
        }

        #endregion
        #endregion

        #region 099_基础运算
        private double Sum(IEnumerable<double> num)
        {
            double sum = 0d;
            foreach (var item in num) { sum += item; }
            return sum;
        }

        private int Sum(IEnumerable<int> num)
        {
            int sum = 0;
            foreach (var item in num) { sum += item; }
            return sum;
        }
        #endregion
        #endregion
    }

    #region 结果数据

    public class ResultFromAll 
    {
        public double PopulationAll { get; set; }
        public IEnumerable<double> Population { get; set; }

        public double[] EConsumptionAll { get; set; }
        public IEnumerable<int[]> EConsumption { get; set; }
        public double[] WConsumptionAll { get; set; }
        public IEnumerable<int[]> WConsumption { get; set; }
        public double[] GConsumptionAll { get; set; }
        public IEnumerable<int[]> GConsumption { get; set; }


        public double[] ConstructionCostALL { get; set; }
        public IEnumerable<ResultFromConstructionCost> ConstructionCost { get; set; }
        public Dictionary<string, double[]> ConstructionCostEachField { get; set; }

        public ResultFromAll(IEnumerable<double> population, IEnumerable<int[]> EC, IEnumerable<int[]> WC, IEnumerable<int[]> GC, List<ResultFromConstructionCost> constructionCost, List<int[]> CCList)
        {
            Population = population;
            EConsumption = EC;
            WConsumption = WC;
            GConsumption = GC;
            ConstructionCost = constructionCost;

            PopulationAll = Population.Sum();
            EConsumptionAll = Consumption_BrepSum(EC);
            WConsumptionAll = Consumption_BrepSum(WC);
            GConsumptionAll = Consumption_BrepSum(GC);
            ConstructionCostALL = Consumption_BrepSum(CCList);
            ConstructionCostEachField = ConstructionCost_SUM(constructionCost);
        }

        public ResultFromAll(IEnumerable<double> population, IEnumerable<int[]> EC, IEnumerable<int[]> WC, IEnumerable<int[]> GC, List<ResultFromConstructionCost> constructionCost, List<double[]> CCList)
        {
            Population = population;
            EConsumption = EC;
            WConsumption = WC;
            GConsumption = GC;
            ConstructionCost = constructionCost;

            PopulationAll = Population.Sum();
            EConsumptionAll = Consumption_BrepSum(EC);
            WConsumptionAll = Consumption_BrepSum(WC);
            GConsumptionAll = Consumption_BrepSum(GC);
            ConstructionCostALL = Consumption_BrepSum(CCList);
            ConstructionCostEachField = ConstructionCost_SUM(constructionCost);
        }

        private double[] Consumption_BrepSum(IEnumerable<int[]> IList)
        {
            var GConsumption = new double[3];
            var len = IList.Count();
            var minList = new List<int>(len);
            var maxList = new List<int>(len);
            for (int i = 0; i < len; i++)
            {
                int[] valueCount = IList.ElementAt(i);
                minList.Add(valueCount[0]);
                maxList.Add(valueCount[1]);
            }
            double minResult = minList.Sum();
            double maxResult = maxList.Sum();

            GConsumption[0] = minResult;
            GConsumption[1] = maxResult;
            GConsumption[2] = (minResult+maxResult)/2;

            return GConsumption;
        }

        private double[] Consumption_BrepSum(IEnumerable<double[]> IList)
        {
            double[] GConsumption = new double[3];
            var len = IList.Count();
            var minList = new List<double>(len);
            var maxList = new List<double>(len);
            for (int i = 0; i < len; i++)
            {
                var valueCount = IList.ElementAt(i);
                minList.Add(valueCount[0]);
                maxList.Add(valueCount[1]);
            }
            var minResult = minList.Sum();
            var maxResult = maxList.Sum();

            GConsumption[0] = minResult;
            GConsumption[1] = maxResult;
            GConsumption[2] = (minResult + maxResult) / 2;

            return GConsumption;
        }

        private Dictionary<string, double[]> ConstructionCost_SUM(IEnumerable<ResultFromConstructionCost> ccList)
        {
            Dictionary<string, List<double[]>> temp_ccList = new Dictionary<string, List<double[]>>();
            var length = ccList.Count();
            var minCostList = new List<double>(length);
            var averageCostList = new List<double>(length);
            var maxCostList = new List<double>(length);

            for (int i = 0; i < length; i++)
            {
                double[] price = new double[3];
                price[0] = ccList.ElementAt(i).CostMin;
                price[1] = ccList.ElementAt(i).CostAverage;
                price[2] = ccList.ElementAt(i).CostMax;
                if (temp_ccList.ContainsKey(ccList.ElementAt(i).Name))
                {
                    temp_ccList[ccList.ElementAt(i).Name].Add(price);
                }
                else
                {
                    temp_ccList.Add(ccList.ElementAt(i).Name, new List<double[]> { price });
                }
            }

            for (int i = 0; i < temp_ccList.Keys.Count; i++)
            {
                var keyName = temp_ccList.Keys.ElementAt(i);
                for (int j = 0; j < temp_ccList[keyName].Count; j++)
                {
                    minCostList.Add(temp_ccList[keyName][j][0]);
                    averageCostList.Add(temp_ccList[keyName][j][1]);
                    maxCostList.Add(temp_ccList[keyName][j][2]);
                }
            }

            var result = new Dictionary<string, double[]>();
            for (int i = 0; i < temp_ccList.Keys.Count; i++)
            {
                result.Add(temp_ccList.Keys.ElementAt(i), new double[] { minCostList.Sum(), averageCostList.Sum(), maxCostList.Sum() });
            }
            return result;
        }

        private Dictionary<string, double[]> ConstructionCost_SUM(List<ResultFromConstructionCost> ccList)
        {
            var temp_ccList = new Dictionary<string, List<double[]>>();
            var length = ccList.Count;
            var minCostList = new List<double>(length);
            var averageCostList = new List<double>(length);
            var maxCostList = new List<double>(length);

            for (int i = 0; i < length; i++)
            {
                var price = new double[3];
                price[0] = ccList[i].CostMin;
                price[1] = ccList[i].CostAverage;
                price[2] = ccList[i].CostMax;
                if (temp_ccList.ContainsKey(ccList[i].Name))
                {
                    temp_ccList[ccList[i].Name].Add(price);
                }
                else
                {
                    temp_ccList.Add(ccList[i].Name, new List<double[]> { price });
                }
            }

            for (int i = 0; i < temp_ccList.Keys.Count; i++)
            {
                var keyName = temp_ccList.Keys.ElementAt(i);
                for (int j = 0; j < temp_ccList[keyName].Count; j++)
                {
                    minCostList.Add(temp_ccList[keyName][j][0]);
                    averageCostList.Add(temp_ccList[keyName][j][1]);
                    maxCostList.Add(temp_ccList[keyName][j][2]);
                }
            }

            var result = new Dictionary<string, double[]>();
            for (int i = 0; i < temp_ccList.Keys.Count; i++)
            {
                result.Add(temp_ccList.Keys.ElementAt(i), new double[] { minCostList.Sum(), averageCostList.Sum(), maxCostList.Sum() });
            }
            return result;
        }

    }
    public class ResultFromConstructionCost
    {
        public string Name { get; set; }
        public double CostMin { get; set; }
        public double CostMax { get; set; }
        public double CostAverage { get; set; }

        public ResultFromConstructionCost(string name, double costMin, double costMax, double costAverage)
        {
            Name = name;
            CostMin = costMin;
            CostMax = costMax;
            CostAverage = costAverage;
        }
    }
    public class ResultFromEWGComsuptionCost
    {
        public string Name { get; set; }
        public int CostMin { get; set; }
        public int CostMax { get; set; }
        public int CostAverage { get; set; }

        public ResultFromEWGComsuptionCost(string name, int costMin, int costMax, int costAverage)
        {
            Name = name;
            CostMin = costMin;
            CostMax = costMax;
            CostAverage = costAverage;
        }
    }

    #endregion
    #region 数据库基础数据

    #region EWGConsumption 基础数据
    public struct BaseEWGConsumption
    {
        public string id { get; set; }
        public string funcName { get; set; }
        public string relativeName { get; set; }
        public double c_max { get; set; }
        public double c_min { get; set; }

        public BaseEWGConsumption(string Id, string FuncName, string RelativeName, double Max, double Min)
        {
            id = Id;
            funcName = FuncName;
            relativeName = RelativeName;
            c_max = Max;
            c_min = Min;
        }
    }
    #endregion

    #region Population 基础数据
    public struct BasePopulation
    {
        public string id { get; set; }
        public double layerMin { get; set; }
        public double layerMax { get; set; }
        public int people { get; set; }
        public double farMin { get; set; }
        public double farMax { get; set; }
        public double maxDensity { get; set; }
        public double minGreen { get; set; }
        public double maxHeight { get; set; }
        public string funcName { get; set; }
        public string relativeName { get; set; }

        public BasePopulation(string Id, double LayerMin, double LayerMax, int People, double FARMin, double FARMax,
            double MaxDensity, double MinGreen, double MaxHeight, string FuncName, string RelativeName
            )
        {
            id = Id;
            layerMin = LayerMin;
            layerMax = LayerMax;
            people = People;
            farMin = FARMin;
            farMax = FARMax;
            maxDensity = MaxDensity;
            minGreen = MinGreen;
            maxHeight = MaxHeight;
            funcName = FuncName;
            relativeName = RelativeName;
        }
    }
    #endregion

    #region Construction cost 基础数据
    public class FuncionClass
    {
        public Dictionary<string, List<Quality>> FuncInfoDic;

        public string CityName { get; set; }
        public int Year { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        private Quality qualityData { get; set; }

        public FuncionClass(List<string[]> resultList)
        {
            CityName = resultList[0][3];
            Year = int.Parse(resultList[0][6]);
            Lat = double.Parse(resultList[0][7]);
            Lon = double.Parse(resultList[0][8]);
            FuncInfoDic = DecodeList(resultList);
        }

        private Dictionary<string, List<Quality>> DecodeList(List<string[]> resultList)
        {
            Dictionary<string, List<Quality>> funcSortDicInside = new Dictionary<string, List<Quality>>();
            Dictionary<string, List<string[]>> funcDic = new Dictionary<string, List<string[]>>();
            var buildingFunctionName = new string[] { "office", "shopping_center", "residential_highRise", "residential_house", "hotel", "industrial", "carpark" };

            for (int i = 0; i < resultList.Count; i++)
            {
                if (funcDic.ContainsKey(resultList[i][0]))
                {
                    funcDic[resultList[i][0]].Add(resultList[i]);
                }
                else
                {
                    funcDic.Add(resultList[i][0], new List<string[]> { resultList[i] });
                }
            }


            for (int i = 0; i < buildingFunctionName.Length; i++)
            {
                var listCount = funcDic[buildingFunctionName[i]].Count;
                List<Quality> qualityList = new List<Quality>(listCount);
                for (int j = 0; j < listCount; j++)
                {
                    var singeResultList = funcDic[buildingFunctionName[i]][j];
                    Quality singleQuality = new Quality(
                        singeResultList[1],
                        int.Parse(singeResultList[2]),
                        double.Parse(singeResultList[4]),
                        double.Parse(singeResultList[5]));

                    qualityList.Add(singleQuality);
                }
                funcSortDicInside.Add(buildingFunctionName[i], qualityList);
            }
            return funcSortDicInside;
        }
    }
    public struct Quality
    {
        public string name { get; }
        public int nameId { get; }
        public double priceMax { get; }
        public double priceMin { get; }

        public Quality(string Name, int NameId, double PriceMax, double PriceMin)
        {
            name = Name;
            nameId = NameId;
            priceMax = PriceMax;
            priceMin = PriceMin;
        }

    }
    #endregion

    #endregion

    #region JSON读取数据
    public class ExtractDataFromJSON
    {
        public List<string> funcList { get; set; }
        public List<int> layerList { get; set; }
        public List<double> areaList { get; set; }
        public ExtractDataFromJSON(string jsonFilePath)
        {
            ReadJson(jsonFilePath);
        }
        private void ReadJson(string jsonFilePath)
        {
            StreamReader sr = File.OpenText(jsonFilePath);

            var feactureCollection = GeoJsonReader.GetFeatureCollectionFromJson(sr.ReadToEnd());
            var length = feactureCollection.Count;
            funcList = new List<string>(length);
            layerList= new List<int>(length);
            areaList = new List<double>(length);

            for (int i = 0; i < length; i++)
            {
                funcList.Add(feactureCollection[i].Attributes["function"].ToString());
                layerList.Add(int.Parse(feactureCollection[i].Attributes["floors"].ToString()));
                areaList.Add(double.Parse(feactureCollection[i].Attributes["totalArea"].ToString())/layerList[i]);
            }
        }
    }


    #endregion
}

