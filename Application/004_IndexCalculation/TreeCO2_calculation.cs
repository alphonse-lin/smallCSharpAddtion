using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UrbanX.Application;

namespace UrbanX.Application
{
    public class TreeCO2_calculation
    {
        private string connectionString { get; set; }
        private List<double> dbhList{ get; set; }
        private int count { get; set; }


        public TreeIndexClass treeIndex { get; set; }
        public List<double> volumeList { get; set; }
        /// <summary>
        /// Dried-Weighted Biomass
        /// </summary>
        public List<double> DWBList { get; set; }
        public List<double> CO2List { get; set; }

        public double totalVolume { get; set; }
        public double totalDWB { get; set; }
        public double totalCO2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="TreeName">tree categories</param>
        /// <param name="DBH">diameter at breast height</param>
        public TreeCO2_calculation(string ConnectionString, string TreeName, List<double> DBH)
        {
            connectionString = ConnectionString;
            dbhList = DBH;
            count = DBH.Count;

            treeIndex = ExtractCostData(TreeName);
            volumeList = CalculateVolume();
            DWBList = CalculateDWB(volumeList);
            CO2List = CalculateTotalCO2(DWBList);

            totalVolume = volumeList.Sum();
            totalDWB = DWBList.Sum();
            totalCO2 = CO2List.Sum();
        }

        private TreeIndexClass ExtractCostData(string treeName)
        {
            int attrCount = 13;
            string sql = string.Format($"select * from tree_co2_index where name='{treeName}';");

            var resultList = DB_Manager.GetData(connectionString, sql, attrCount);
            var treeClassInfo = new TreeIndexClass(resultList);

            return treeClassInfo;
        }

        private List<double> CalculateVolume()
        {
            var result = new List<double>(count);
            for (int i = 0; i < dbhList.Count; i++)
            {
                var volume = Math.Pow(treeIndex.ve_index_01 * dbhList[i], treeIndex.ve_index_02);
                result.Add(volume);
            }
            return result;
        }

        private List<double> CalculateDWB(List<double> volumeList)
        {
            var result = new List<double>(count);
            for (int i = 0; i < dbhList.Count; i++)
            {
                var dwb = volumeList[i] * treeIndex.dw_density * treeIndex.root_index;
                result.Add(dwb);
            }
            return result;
        }

        private List<double> CalculateTotalCO2(List<double> dwbList)
        {
            var result = new List<double>(count);
            for (int i = 0; i < dbhList.Count; i++)
            {
                var co2 = dwbList[i] * treeIndex.dwb_c_index*treeIndex.c_co2_weight_ratio;
                result.Add(co2);
            }
            return result;
        }


    }

    public struct TreeIndexClass
    {
        public int id { get; set; }
        public string name { get; set; }
        public string latin_name { get; set; }
        public int dbh_min { get; set; }
        public int dbh_max { get; set; }
        public double ve_index_01 { get; set; }
        public double ve_index_02 { get; set; }
        public double htm_index { get; set; }
        public int dw_density { get; set; }
        public string equation_source { get; set; }
        public double root_index { get; set; }
        public double dwb_c_index { get; set; }
        public double c_co2_weight_ratio { get; set; }

        public TreeIndexClass(List<string[]> resultList)
        {
            var treeIndex = resultList[0];

            id = int.Parse(treeIndex[0]);
            name = treeIndex[1];
            latin_name = treeIndex[2];
            dbh_min = int.Parse(treeIndex[3]);
            dbh_max = int.Parse(treeIndex[4]);
            ve_index_01 = double.Parse(treeIndex[5]);
            ve_index_02 = double.Parse(treeIndex[6]);
            htm_index = double.Parse(treeIndex[7]);
            dw_density = int.Parse(treeIndex[8]);
            equation_source = treeIndex[9];
            root_index = double.Parse(treeIndex[10]);
            dwb_c_index = double.Parse(treeIndex[11]);
            c_co2_weight_ratio = double.Parse(treeIndex[12]);

        }
    }
}
