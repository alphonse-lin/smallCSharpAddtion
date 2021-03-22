using CliWrap;
using CliWrap.Buffered;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanX.Application;

namespace UrbanX.Application
{
    public class DB_Manager
    {
        #region 示例
        //public static void CombinedData(string connectionString)
        //{
        //    #region 读取数据库
        //    var attrName_read = "name";
        //    var tableName_read = "construction_cost";

        //    var strResult = GetData(connectionString, attrName_read, tableName_read);
        //    #endregion

        //    #region 创建数据
        //    var tableName_insert = "construction_cost";
        //    var attrName_insert = "quality_name_id";

        //    Dictionary<string, int> qualityNameDic = new Dictionary<string, int>()
        //        {
        //            { "high_quality",1},{ "medium",2},{ "low",3},{ "clubhouse",4},
        //            { "external_work",5},{ "5_star",6},{ "3_star",7},{ "landlord",8},
        //            { "end_user",9},{ "basement",10},{ "multi_story",11}
        //        };

        //    string[] attrNameArray = new string[strResult.Count];
        //    int[] insertValue = new int[strResult.Count];
        //    for (int i = 0; i < insertValue.Length; i++)
        //    {
        //        attrNameArray[i] = attrName_insert;
        //        insertValue[i] = qualityNameDic[strResult[i]];
        //    }

        //    Dictionary<string, FuncionClass> cityInfo_pg = new Dictionary<string, FuncionClass>();
        //    #endregion

        //    #region 录入数据库
        //    string sql = String.Format("insert into [{0}]([{1}])values(@{1})", tableName_insert, attrName_insert);
        //    var insertResult = InsertData(connectionString, sql, tableName_read, attrNameArray, insertValue);
        #endregion

        #region 数据库模块

        #region 通用数据库模块

        #region 检测是否连接
        public static string Connection()
        {
            string str = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlabdb";
            string strMessage = string.Empty;
            try

            {
                NpgsqlConnection conn = new NpgsqlConnection(str);
                conn.Open();
                strMessage = "Success";
                conn.Close();
            }
            catch
            {
                strMessage = "Failure";
            }
            return strMessage;
        }
        #endregion
        #region 读取数据库
        public static List<string> GetData(string connectStr, string attrName, string tableName)
        {
            var intArray = new int[] { attrName.Split(",").Length };
            List<string> strResult = new List<string>();
            for (int i = 0; i < attrName.Split(",").Length; i++) { intArray[i] = i; }
            try
            {
                IDbConnection dbcon;
                dbcon = new NpgsqlConnection(connectStr);
                dbcon.Open();
                IDbCommand dbcmd = dbcon.CreateCommand();

                var sqlSearch = String.Format("SELECT {0} FROM {1}", attrName, tableName);
                dbcmd.CommandText = sqlSearch;
                IDataReader dr = dbcmd.ExecuteReader();

                while (dr.Read())
                {
                    for (int i = 0; i < intArray.Length; i++)
                    {
                        strResult.Add(dr[i].ToString().Trim());
                    }
                }

                dr.Close();
                dr = null;
            }
            catch (Exception e) { throw e; }

            return strResult;
        }

        public static List<string[]> GetData(string connectStr, string sqlSearch, int attrCount)
        {
            List<string[]> resultList = new List<string[]>();
            try
            {
                IDbConnection dbcon;
                dbcon = new NpgsqlConnection(connectStr);
                dbcon.Open();
                IDbCommand dbcmd = dbcon.CreateCommand();

                dbcmd.CommandText = sqlSearch;
                IDataReader dr = dbcmd.ExecuteReader();

                while (dr.Read())
                {
                    string[] singleResult = new string[attrCount];
                    for (int i = 0; i < attrCount; i++)
                    {
                        singleResult[i] = dr[i].ToString().Trim();
                    }
                    resultList.Add(singleResult);
                }

                dr.Close();
                dr = null;
            }
            catch (Exception e) { throw e; }
            return resultList;
        }
        #endregion
        #region 录入数据库
        public static int[] InsertData(string connectionString, string sql, string tableName, string[] attrNameArray, string[][] insertValue)
        {
            IDBHelper dbHelper = new PostgreHelper();
            int[] result = new int[insertValue.Length];
            for (int i = 0; i < insertValue.Length; i++)
            {
                var _params = CreateNpgsqlParas(attrNameArray, insertValue[i]);
                int r = dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, sql, _params);
            }
            return result;
        }
        public static int[] InsertData(string connectionString, string sql, string tableName, string[] attrNameArray, int[] insertValue)
        {
            IDBHelper dbHelper = new PostgreHelper();
            int[] result = new int[insertValue.Length];
            for (int i = 0; i < insertValue.Length; i++)
            {
                var _params = CreateNpgsqlParas(attrNameArray, insertValue);
                int r = dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, sql, _params);
            }
            return result;
        }

        public static NpgsqlParameter[] CreateNpgsqlParas(string[] attrNameArray, string[] insertValue)
        {
            NpgsqlParameter[] result = new NpgsqlParameter[attrNameArray.Length];
            for (int i = 0; i < attrNameArray.Length; i++)
            {
                var tempInsertValue_1 = insertValue[i];
                if (double.TryParse(tempInsertValue_1, out double double_tempInsertValue))
                {
                    if (int.TryParse(tempInsertValue_1,out int int_tempInsertValue))
                        result[i] = new NpgsqlParameter(String.Format("@{0}", attrNameArray[i]), int_tempInsertValue);
                    else
                        result[i] = new NpgsqlParameter(String.Format("@{0}", attrNameArray[i]), double_tempInsertValue);
                }
                else
                    result[i] = new NpgsqlParameter(String.Format("@{0}", attrNameArray[i]), tempInsertValue_1);
                
            }
            return result;
        }

        public static NpgsqlParameter[] CreateNpgsqlParas(string[] attrNameArray, int[] insertValue)
        {
            NpgsqlParameter[] result = new NpgsqlParameter[attrNameArray.Length];
            for (int i = 0; i < attrNameArray.Length; i++)
            {
                result[i] = new NpgsqlParameter(String.Format("@{0}", attrNameArray[i]), insertValue[i]);
            }
            return result;
        }

        /// <summary>
        /// unused
        /// </summary>
        public static void InsertData(string connectionString)
        {
            IDBHelper dbHelper = new PostgreHelper();

            string sql = "insert into [building_functions]([name])values(@name)";
            var buildingFunctionName = new string[] { "office", "shopping_center", "residential_highRise", "residential_house", "hotel", "industrial", "carpark" };

            for (int i = 0; i < buildingFunctionName.Length; i++)
            {
                var _params = new NpgsqlParameter[] {
                new NpgsqlParameter("@name", buildingFunctionName[i]),
                };
                int r = dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, sql, _params);
            }
        }
        #endregion
        public async static Task<bool> OutputSHPtoPSGL(string shpfile, string host, string user, string pwd, string db, string schema = "public", bool dropIfExists = true, string table = "[SHPNAME]")
        {
            FileInfo shp = new FileInfo(shpfile);
            if (!shp.Exists) return false;
            if (table == "[SHPNAME]") table = Path.GetFileNameWithoutExtension(shpfile).ToLower();

            string args = string.Format("{0} {1}.{2}", shpfile, schema, table);

            var result = await Cli.Wrap(@"D:\PostGreSQL_12\bin\shp2pgsql.exe").WithArguments(args).ExecuteBufferedAsync(Encoding.UTF8);

            string sql = result.StandardOutput;
            string constring = string.Format("Host={0};Username={1};Password={2};Database={3}", host, user, pwd, db);
            if (dropIfExists) sql = sql.Replace("CREATE TABLE", string.Format("DROP TABLE IF EXISTS \"{0}\".\"{1}\";\r\nCREATE TABLE", schema, table));

            using (NpgsqlConnection connection = new NpgsqlConnection(constring))
            {
                connection.Open();
                new NpgsqlCommand(sql, connection).ExecuteNonQuery();
            }
            return true;
        }

        #endregion

        #region 特定数据库模块

        #region 抽取类型数据
        public static List<NpgsqlParameter[]> ExtractCityConstructionCost(ConstructionCostClass cityInfo)
        {
            var cityName = cityInfo.CityName;
            var adcode = GaodeLocation.DecodeResult(cityInfo.CityName).adcode;
            const int year = 2020;
            Dictionary<int, string[]> functionDis = new Dictionary<int, string[]> {
                { 1, new string[4] { "office", "high_quality", "medium", "low" } },
                { 2, new string[3] { "shopping_center", "high_quality", "medium"} } ,
                { 3, new string[4] { "residential_highrise", "high_quality", "medium", "low" } } ,
                { 4, new string[5] { "residential_house", "high_quality", "medium", "clubhouse","external_work" } } ,
                { 5, new string[3] { "hotel", "5_star", "3_star" } },
                { 6, new string[3] { "industrial", "landlord", "end_user" } },
                { 7, new string[3] { "carpark", "basement", "multi_story"} },
            };

            var officeList = ExtractFromCityInfo(functionDis, cityInfo.Office, cityName, year, adcode, 0);
            var shopping_centerList = ExtractFromCityInfo(functionDis, cityInfo.Shopping, cityName, year, adcode, 1);
            var residential_highriseList = ExtractFromCityInfo(functionDis, cityInfo.Residential_HighRise, cityName, year, adcode, 2);
            var residential_houseList = ExtractFromCityInfo(functionDis, cityInfo.Residential_House, cityName, year, adcode, 3);
            var hotelList = ExtractFromCityInfo(functionDis, cityInfo.Hotel, cityName, year, adcode, 4);
            var industrialList = ExtractFromCityInfo(functionDis, cityInfo.Industrial, cityName, year, adcode, 5);
            var carparkList = ExtractFromCityInfo(functionDis, cityInfo.Carpark, cityName, year, adcode, 6);

            var npgsqlParaList = new List<NpgsqlParameter[]>();
            npgsqlParaList.AddRange(officeList);
            npgsqlParaList.AddRange(shopping_centerList);
            npgsqlParaList.AddRange(residential_highriseList);
            npgsqlParaList.AddRange(residential_houseList);
            npgsqlParaList.AddRange(hotelList);
            npgsqlParaList.AddRange(industrialList);
            npgsqlParaList.AddRange(carparkList);

            return npgsqlParaList;
        }
        public static List<NpgsqlParameter[]> ExtractFromCityInfo(Dictionary<int, string[]> functionDis, string[][] functionInfo, string cityName, int year, string adcode, int num)
        {
            var paraList = new List<NpgsqlParameter[]>();
            Dictionary<string, int> qualityNameDic = new Dictionary<string, int>()
                {
                    { "high_quality",1},{ "medium",2},{ "low",3},{ "clubhouse",4},
                    { "external_work",5},{ "5_star",6},{ "3_star",7},{ "landlord",8},
                    { "end_user",9},{ "basement",10},{ "multi_story",11}
                };

            for (int i = 0; i < functionInfo.Length; i++)
            {
                if (ToolManagers.IsNumeric(functionInfo[i][0], out _))
                {
                    var _params = new NpgsqlParameter[] {
                new NpgsqlParameter("@year", 2020),
                new NpgsqlParameter("@city_id", adcode),
                new NpgsqlParameter("@func_id", functionDis.Keys.ElementAt(num)),
                new NpgsqlParameter("@price_min", double.Parse(functionInfo[i][0])),
                new NpgsqlParameter("@price_max",  double.Parse(functionInfo[i][1])),
                new NpgsqlParameter("@quality_id",qualityNameDic[functionDis[functionDis.Keys.ElementAt(num)][i+1]])
                    };
                    paraList.Add(_params);
                }
                else
                {
                    var _params = new NpgsqlParameter[] {
                new NpgsqlParameter("@year", 2020),
                new NpgsqlParameter("@city_id", adcode),
                new NpgsqlParameter("@func_id", functionDis.Keys.ElementAt(num)),
                new NpgsqlParameter("@price_min", -1),
                new NpgsqlParameter("@price_max", -1),
                new NpgsqlParameter("@quality_id",qualityNameDic[functionDis[functionDis.Keys.ElementAt(num)][i+1]] )};
                    paraList.Add(_params);
                }

            }
            return paraList;
        }
        #endregion
        
        #endregion
        #endregion
    }
}
