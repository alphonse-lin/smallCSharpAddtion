using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Xml;
using System.IO;
using System.Data.OleDb;
using System.Xml.Linq;
using Npgsql;
using utility.DataBase;
using utility.DecodeAddress;

namespace utility
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 读取excel数据，转为citiesCostModel 类
            //string excelPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\utility\data\造价表2.xlsx";
            //string xmlPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\utility\data\test_1124.xml";

            //XMLManager.Excel2Xml(excelPath, xmlPath, "Sheet2");
            //var citiesCostModelList = XMLManager.xmlParseCities(xmlPath);

            //foreach (var item in citiesCostModelList)
            //{
            //    Console.WriteLine($"1. cityName:{item.CityName}\n");
            //    Console.WriteLine($"2. cityId:{item.CityID}\n");
            //    Console.WriteLine($"3. cityOfficeHigh:{item.Office[0][0]},{item.Office[0][1]}\n");
            //    Console.WriteLine($"4. cityShoppingHigh:{item.Shopping[0][0]},{item.Shopping[0][1]}\n");
            //    Console.WriteLine($"5. cityHotelHigh:{item.Hotel[0][0]},{item.Hotel[0][1]}\n");
            //    Console.WriteLine($"6. cityResidentialHigh:{item.Residential_HighRise[0][0]},{item.Residential_HighRise[0][1]}\n");
            //    Console.WriteLine($"7. cityIndustrialHigh:{item.Industrial[0][0]},{item.Industrial[0][1]}\n");
            //    Console.WriteLine($"8. cityCarparkHigh:{item.Carpark[0][0]},{item.Carpark[0][1]}\n");
            //}

            //Console.WriteLine("finished");
            //Console.ReadLine();
            #endregion

            #region 第一次连接数据库PGSQL
            //var cs = "Host=127.0.0.1;Username=postgres;Password=admin;Database=test";

            //var con = new NpgsqlConnection(cs);
            //con.Open();

            //var sql = "SELECT version()";

            //var cmd = new NpgsqlCommand(sql, con);

            //var version = cmd.ExecuteScalar().ToString();
            //Console.WriteLine($"PostgreSQL version: {version}");
            //Console.ReadKey();
            #endregion

            #region 第二次链接数据库 创建表格, 插入数据
            //var cs = "Host=127.0.0.1;Username=postgres;Password=admin;Database=test";
            //var con = new NpgsqlConnection(cs);
            //con.Open();

            //var cmd = new NpgsqlCommand();
            //cmd.Connection = con;

            //cmd.CommandText = @"DROP TABLE IF EXISTS cars";
            //cmd.ExecuteNonQuery();

            //cmd.CommandText = @"CREATE TABLE CARS(ID SERIAL PRIMARY KEY,
            //      name VARCHAR(255), price INT)";
            //cmd.ExecuteNonQuery();

            //cmd.CommandText="INSERT INTO cars(name, price) VALUES('Audi', 52642)";
            //cmd.ExecuteNonQuery();

            //cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Mercedes',57127)";
            //cmd.ExecuteNonQuery();

            //cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Skoda',9000)";
            //cmd.ExecuteNonQuery();

            //cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Volvo',29000)";
            //cmd.ExecuteNonQuery();

            //cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Bentley',350000)";
            //cmd.ExecuteNonQuery();

            //cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Citroen',21000)";
            //cmd.ExecuteNonQuery();

            //cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Hummer',41400)";
            //cmd.ExecuteNonQuery();

            //cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Volkswagen',21600)";
            //cmd.ExecuteNonQuery();

            //Console.WriteLine("Table cars created");
            #endregion

            #region 第三次链接数据库 prepared statement
            //var cs = "Host=127.0.0.1;Username=postgres;Password=admin;Database=test";
            //var con = new NpgsqlConnection(cs);
            //con.Open();

            //var sql = "INSERT INTO cars(name, price) VALUES(@name, @price)";
            //var cmd = new NpgsqlCommand(sql, con);

            //cmd.Parameters.AddWithValue("name", "BMW");
            //cmd.Parameters.AddWithValue("price", 36600);
            //cmd.Prepare();

            //cmd.ExecuteNonQuery();
            //Console.WriteLine("row inserted");

            #endregion

            #region 第四次链接数据库 读取数据
            //var cs = "Host=127.0.0.1;Username=postgres;Password=admin;Database=test";
            //var con = new NpgsqlConnection(cs);
            //con.Open();

            //var sql = "SELECT * FROM cars";
            //var cmd = new NpgsqlCommand(sql, con);

            //NpgsqlDataReader rdr = cmd.ExecuteReader();

            //while (rdr.Read())
            //{
            //    Console.WriteLine("{0} {1} {2}",
            //        rdr.GetInt32(0), rdr.GetString(1), rdr.GetInt32(2));
            //}
            #endregion

            #region 第五次链接数据库 column headers
            //var cs = "Host=127.0.0.1;Username=postgres;Password=admin;Database=test";
            //var con = new NpgsqlConnection(cs);
            //con.Open();

            //var sql = "SELECT * FROM cars";
            //var cmd = new NpgsqlCommand(sql, con);
            //NpgsqlDataReader rdr = cmd.ExecuteReader();
            //Console.WriteLine($"{rdr.GetName(0),-4} {rdr.GetName(1),-10} {rdr.GetName(2),10}");

            //while (rdr.Read())
            //{
            //    Console.WriteLine($"{rdr.GetInt32(0),-4}{rdr.GetString(1),-10}{rdr.GetInt32(2),10}");
            //}
            #endregion

            #region 第六次正式运行，录入城市数据
            //IDBHelper dbHelper = new PostgreHelper();
            ////string connectionString = "Host=127.0.0.1;Username=postgres;Password=admin;Database=test";
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlabdb";

            //var createDB = "create table if not exists cities(id serial, code char(20) primary key, name char(20), lat numeric(9,6), lon numeric(9,6)) ";
            //dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, createDB);

            //string sql = "insert into [cities]([code],[name],[lat],[lon])values(@code, @name, @lat, @lon)";

            //var cityNames = new string[] { "北京", "成都", "广州", "上海", "深圳", "天津", "武汉","无锡","西安","珠海"};
            ////var cityNames = new string[] { "杭州", "厦门", "东莞", "重庆", "福州", "泉州", "太原", "大同", "漳州", "佛山" };
            //var cityInfoList = new List<GeocodeResult>(cityNames.Length);
            //for (int i = 0; i < cityNames.Length; i++)
            //    cityInfoList.Add(GaodeLocation.DecodeResult(cityNames[i]));

            //for (int i = 0; i < cityInfoList.Count; i++)
            //{
            //    var _params = new NpgsqlParameter[] {
            //    new NpgsqlParameter("@code", cityInfoList[i].adcode),
            //    new NpgsqlParameter("@name", cityNames[i]),
            //    new NpgsqlParameter("@lat", cityInfoList[i].latitude),
            //    new NpgsqlParameter("@lon", cityInfoList[i].lontitude),
            //    };
            //    int r = dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, sql, _params);
            //}


            #endregion

            #region 第七次正式运行，录入类型数据
            //IDBHelper dbHelper = new PostgreHelper();
            ////string connectionString = "Host=127.0.0.1;Username=postgres;Password=admin;Database=test";
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlabdb";

            //var createDB = "create table if not exists building_functions(id serial primary key, name char(40)) ";
            //dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, createDB);

            //string sql = "insert into [building_functions]([name])values(@name)";

            ////string excelPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\utility\data\造价表2.xlsx";
            ////string xmlPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\utility\data\test_1124.xml";

            ////XMLManager.Excel2Xml(excelPath, xmlPath, "Sheet2");
            ////var citiesCostModelList = XMLManager.xmlParseCities(xmlPath);

            //var buildingFunctionName = new string[] { "office", "shopping_center", "residential_highRise", "residential_house", "hotel", "industrial", "carpark" };

            //for (int i = 0; i < buildingFunctionName.Length; i++)
            //{
            //    var _params = new NpgsqlParameter[] {
            //    new NpgsqlParameter("@name", buildingFunctionName[i]),
            //    };
            //    int r = dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, sql, _params);
            //}

            ////Console.WriteLine("完成");
            #endregion

            #region 第八次录入数据库 成本库
            IDBHelper dbHelper = new PostgreHelper();
            //string connectionString = "Host=127.0.0.1;Username=postgres;Password=admin;Database=test";
            string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlabdb";

            var createDB = "create table if not exists construction_cost(" +
                "id serial,\n " +
                "name char(40),\n" +
                "year int,\n" +
                "city_id char(20),\n" +
                "func_id int,\n" +
                "price_max numeric(9,2),\n" +
                "price_min numeric(9,2),\n" +
                "quality_id int,\n" +


                "Primary Key(id),\n" +

                "Constraint fk_city_id\n" +
                "Foreign Key(city_id)\n" +
                "References cities(code),\n" +

                "Constraint fk_func_id\n" +
                "Foreign Key(func_id)\n" +
                "References building_functions(id)," +

                "Constraint fk_quality_id\n" +
                "Foreign Key(quality_id)\n" +
                "References quality_functions(quality_id)" +
                ") ";
            dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, createDB);

            string sql = "insert into [construction_cost]([name],[year],[city_id],[func_id],[price_min],[price_max])values(@name,@year,@city_id,@func_id,@price_min,@price_max)";

            string excelPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\utility\data\造价表2.xlsx";
            string xmlPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\utility\data\test_1124.xml";

            XMLManager.Excel2Xml(excelPath, xmlPath, "Sheet2");
            var citiesCostModelList = XMLManager.xmlParseCities(xmlPath);


            for (int i = 0; i < citiesCostModelList.Count; i++)
            {
                var resultList = ExtractCityConstructionCost(citiesCostModelList[i]);
                for (int j = 0; j < resultList.Count; j++)
                {
                    int r = dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, sql, resultList[j]);
                }
            };

            Console.WriteLine("完成");
            #endregion

            #region 地理位置编码
            //var data = GaodeLocation.DecodeResult("成都");
            //Console.WriteLine(data.adcode +"\n"+data.latitude+ "\n"+data.lontitude);
            #endregion

            Console.ReadLine();

            
        }

        private static List<NpgsqlParameter[]> ExtractCityConstructionCost(ConstructionCostClass cityInfo)
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
            var shopping_centerList= ExtractFromCityInfo(functionDis, cityInfo.Shopping, cityName, year, adcode,1);
            var residential_highriseList = ExtractFromCityInfo(functionDis, cityInfo.Residential_HighRise, cityName, year, adcode, 2);
            var residential_houseList= ExtractFromCityInfo(functionDis, cityInfo.Residential_House, cityName, year, adcode, 3);
            var hotelList= ExtractFromCityInfo(functionDis, cityInfo.Hotel, cityName, year, adcode, 4);
            var industrialList= ExtractFromCityInfo(functionDis, cityInfo.Industrial, cityName, year, adcode, 5);
            var carparkList= ExtractFromCityInfo(functionDis, cityInfo.Carpark, cityName, year, adcode, 6);

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

        private static List<NpgsqlParameter[]> ExtractFromCityInfo(Dictionary<int, string[]> functionDis, string[][] functionInfo, string cityName, int year, string adcode, int num)
        {
            var paraList = new List<NpgsqlParameter[]>();


            for (int i = 0; i < functionInfo.Length; i++)
            {
                if (IsNumeric(functionInfo[i][0],out _))
                {
                    var _params = new NpgsqlParameter[] {
                new NpgsqlParameter("@name",functionDis[functionDis.Keys.ElementAt(num)][i+1] ),
                new NpgsqlParameter("@year", 2020),
                new NpgsqlParameter("@city_id", adcode),
                new NpgsqlParameter("@func_id", functionDis.Keys.ElementAt(num)),
                new NpgsqlParameter("@price_min", double.Parse(functionInfo[i][0])),
                new NpgsqlParameter("@price_max",  double.Parse(functionInfo[i][1]))};
                    paraList.Add(_params);
                }
                else
                {
                    var _params = new NpgsqlParameter[] {
                new NpgsqlParameter("@name",functionDis[functionDis.Keys.ElementAt(num)][i+1] ),
                new NpgsqlParameter("@year", 2020),
                new NpgsqlParameter("@city_id", adcode),
                new NpgsqlParameter("@func_id", functionDis.Keys.ElementAt(num)),
                new NpgsqlParameter("@price_min", -1),
                new NpgsqlParameter("@price_max", -1)};
                    paraList.Add(_params);
                }
                
            }
            return paraList;
        }

        private static bool IsNumeric(string s, out double result)
        {
            bool bReturn = true;
            try
            {
                result = double.Parse(s);
            }
            catch
            {
                result = 0;
                bReturn = false;
            }
            return bReturn;
        }


        #region 数据结构测试
        private static void CreateStructuredXml(string testPath)
        {
            var doc = new XDocument(
            new XElement("Cities",
                new XElement("City", new XAttribute("Name", "HongKong"),
                    new XElement("Offices",
                        new XElement("HighQuaility", "21700,30900"),
                        new XElement("MediumQuaility", "17900,21800"),
                        new XElement("OrdinaryQuaility", "15300,19200")),
                    new XElement("Hotel",
                        new XElement("HighQuaility", "21700,30900"),
                        new XElement("MediumQuaility", "17900,21800"),
                        new XElement("OrdinaryQuaility", "15300,19200")
            )),
                new XElement("City", new XAttribute("Name", "北京"),
                    new XElement("Offices",
                        new XElement("HighQuaility", "21700,30900"),
                        new XElement("MediumQuaility", "17900,21800"),
                        new XElement("OrdinaryQuaility", "15300,19200")
            ))));
            doc.Save(testPath);
        }




        #endregion
    }
}
    
