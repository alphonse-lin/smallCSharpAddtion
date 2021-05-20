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
using CsvHelper;
using Npgsql;
using System.Globalization;
using UrbanX.Application.Office;
using UrbanX.Application.Geometry;
using UrbanXX.IO.GeoJSON;
using g3;
using Urbanx.Application.Geometry.Extension;

namespace UrbanX.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime start = System.DateTime.Now;
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
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlab_db";

            //var createDB = "create table if not exists cities(id serial, code char(20) primary key, name char(20), lat numeric(9,6), lon numeric(9,6)) ";
            //dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, createDB);

            //string sql = "insert into [cities]([code],[name],[lat],[lon])values(@code, @name, @lat, @lon)";

            ////var cityNames = new string[] { "北京", "成都", "广州", "上海", "深圳", "天津", "武汉", "无锡", "西安", "珠海" };
            //var cityNames = new string[] { "杭州", "厦门", "东莞", "重庆", "福州", "泉州", "太原", "大同", "漳州", "佛山" };
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
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlab_db";

            ////var createDB = "create table if not exists building_functions(id serial primary key, name char(40)) ";
            ////dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, createDB);

            //string sql = "insert into [building_functions]([name],[relative_name])values(@name,@relative_name)";

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
            //IDBHelper dbHelper = new PostgreHelper();
            ////string connectionString = "Host=127.0.0.1;Username=postgres;Password=admin;Database=test";
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlab_db";

            //var createDB = "create table if not exists construction_cost(" +
            //    "id serial,\n " +
            //    "func_id int,\n" +
            //    "quality_id int,\n" +
            //    "city_id char(20),\n" +
            //    "price_max numeric(9,2),\n" +
            //    "price_min numeric(9,2),\n" +
            //    "year int,\n" +



            //    "Primary Key(id),\n" +

            //    "Constraint fk_city_id\n" +
            //    "Foreign Key(city_id)\n" +
            //    "References cities(code),\n" +

            //    "Constraint fk_func_id\n" +
            //    "Foreign Key(func_id)\n" +
            //    "References building_functions(id)," +

            //    "Constraint fk_quality_id\n" +
            //    "Foreign Key(quality_id)\n" +
            //    "References quality_functions(quality_id)" +
            //    ") ";
            //dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, createDB);

            //string sql = "insert into [construction_cost]([year],[city_id],[func_id],[price_min],[price_max],[quality_id])values(@year,@city_id,@func_id,@price_min,@price_max,@quality_id)";

            //string excelPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\造价表2.xlsx";
            //string xmlPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\test_1124.xml";

            //XMLManager.Excel2Xml(excelPath, xmlPath, "Sheet2");
            //var citiesCostModelList = XMLManager.xmlParseCities(xmlPath);


            //for (int i = 0; i < citiesCostModelList.Count; i++)
            //{
            //    var resultList = DB_Manager.ExtractCityConstructionCost(citiesCostModelList[i]);
            //    for (int j = 0; j < resultList.Count; j++)
            //    {
            //        int r = dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, sql, resultList[j]);
            //    }
            //};

            //Console.WriteLine("完成");
            #endregion

            #region 第九次 读取数据库 成本库
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlab_db";
            //string city = "北京";
            //int attrCount = 9;
            //string sql = string.Format("select " +
            //    "bf.name,qf.quality_name,cc.quality_id,c.name, cc.price_max, cc.price_min,cc.year,c.lat, c.lon " +
            //    "FROM construction_cost cc, cities c, building_functions bf, quality_functions qf " +
            //    "where cc.city_id = c.code and cc.func_id = bf.id and cc.quality_id = qf.quality_id and c.name='{0}'; ", city);

            //var resultList = DB_Manager.GetData(connectionString, sql, attrCount);
            //FuncionClass cityInfo = new FuncionClass(resultList);
            //var result = cityInfo.FuncInfoDic;

            //Console.WriteLine("城市为{0}, 经度为{1},纬度{2}", cityInfo.CityName, cityInfo.Lat.ToString(), cityInfo.Lon.ToString());
            //Console.WriteLine("完成");
            #endregion

            #region 玩真的 标准计算数据

            #region 录入数据库数据 EC/WC/GC
            //    IDBHelper dbHelper = new PostgreHelper();
            //    //string connectionString = "Host=127.0.0.1;Username=postgres;Password=admin;Database=test";
            //    string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlab_db";

            //    var xmlPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\UrbanX\Data\indexCalculation.xml";
            //    var indexData=new IndexCalculation(xmlPath);

            //    string sql = "insert into [building_consumption_value]([cv_name],[building_type],[cv_min],[cv_max])values(@cv_name,@building_type,@cv_min,@cv_max)";
            //    string tablename = "building_consumption_value";
            //    string[] attrName = new string[] {"cv_name","building_type", "cv_max", "cv_min" };

            //    Dictionary<string, int> FunctionName = new Dictionary<string, int> {
            //    { "R", 3},
            //    { "H", 5},
            //    { "O", 1},
            //    { "C", 2},
            //    { "M", 6},
            //    { "W", 8},
            //    { "S", 9},
            //    { "GIC", 10},
            //};

            //    string[][] insertValue = new string[3][];
            //    for (int i = 0; i < indexData.Con_Buildings.Keys.Count; i++)
            //    {
            //        var keyName = indexData.Con_Buildings.Keys.ElementAt(i);
            //        insertValue[0] = new string[4];
            //        insertValue[0][3] = indexData.Con_Buildings[keyName]._EConsumption[0].ToString();
            //        insertValue[0][2] = indexData.Con_Buildings[keyName]._EConsumption[1].ToString();
            //        insertValue[0][1] = FunctionName[keyName].ToString();
            //        insertValue[0][0] = "EConsumption";

            //        insertValue[1] = new string[4];
            //        insertValue[1][3] = indexData.Con_Buildings[keyName]._WConsumption[0].ToString();
            //        insertValue[1][2] = indexData.Con_Buildings[keyName]._WConsumption[1].ToString();
            //        insertValue[1][1] = FunctionName[keyName].ToString();
            //        insertValue[1][0] = "WConsumption";

            //        insertValue[2] = new string[4];
            //        insertValue[2][3] = indexData.Con_Buildings[keyName]._GConsumption[0].ToString();
            //        insertValue[2][2] = indexData.Con_Buildings[keyName]._GConsumption[1].ToString();
            //        insertValue[2][1] = FunctionName[keyName].ToString();
            //        insertValue[2][0] = "GConsumption";
            //        DB_Manager.InsertData(connectionString, sql, tablename, attrName, insertValue);
            //    }
            #endregion

            #region 录入数据库数据 Population
            //IDBHelper dbHelper = new PostgreHelper();
            ////string connectionString = "Host=127.0.0.1;Username=postgres;Password=admin;Database=test";
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlab_db";

            //var xmlPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\UrbanX\Data\indexCalculation.xml";
            //var indexData = new IndexCalculation(xmlPath);

            //var createDB = "create table if not exists building_population(" +
            //    "bp_id serial,\n " +
            //    "bp_name char(20),\n" +
            //    "bp_layer_min int,\n" +
            //    "bp_layer_max int,\n" +
            //    "bp_people int,\n" +
            //    "bp_FAR_min numeric(4,2),\n" +
            //    "bp_FAR_max numeric(4,2),\n" +
            //    "bp_density_max numeric(3,2),\n" +
            //    "bp_green_min numeric(3,2),\n" +
            //    "bp_height_max numeric(3,1),\n" +
            //    "bp_func_id int,\n" +

            //    "Primary Key(bp_id),\n" +

            //    "Constraint fk_func_id\n" +
            //    "Foreign Key(bp_func_id)\n" +
            //    "References building_functions(id)\n" +
            //    ") ";
            //dbHelper.ExecuteNonQuery(connectionString, CommandType.Text, createDB);

            //string sql = "insert into [building_population](" +
            //    "[bp_name],[bp_layer_min],[bp_layer_max],[bp_people],[bp_far_min],[bp_far_max]," +
            //    "[bp_density_max],[bp_green_min],[bp_height_max],[bp_func_id])" +
            //    "values(@bp_name,@bp_layer_min,@bp_layer_max,@bp_people,@bp_far_min,@bp_far_max" +
            //    ",@bp_density_max,@bp_green_min,@bp_height_max,@bp_func_id)";

            //string tablename = "building_population";
            //string[] attrName = new string[] { 
            //    "bp_name", "bp_layer_min", "bp_layer_max", "bp_people", "bp_FAR_min" , "bp_FAR_max",
            //    "bp_density_max","bp_green_min","bp_height_max","bp_func_id"
            //};

            //Dictionary<string, int> FunctionName = new Dictionary<string, int> {
            //    { "R", 3},
            //    { "H", 5},
            //    { "O", 1},
            //    { "C", 2},
            //    { "M", 6},
            //    { "W", 8},
            //    { "S", 9},
            //    { "GIC", 10},
            //};

            //int NodeCount = indexData.PopulationType.Keys.Count;
            //string[][] insertValue = new string[NodeCount][];
            //for (int i = 0; i < NodeCount; i++)
            //{
            //    var keyName = i;
            //    insertValue[i] = new string[10];
            //    insertValue[i][0] = string.Format("type_{0}", i);
            //    insertValue[i][1] = indexData.PopulationType[keyName]._layer[0].ToString();
            //    insertValue[i][2] = indexData.PopulationType[keyName]._layer[1].ToString();
            //    insertValue[i][3] = indexData.PopulationType[keyName]._people.ToString();
            //    insertValue[i][4] = indexData.PopulationType[keyName]._FAR[0].ToString();
            //    insertValue[i][5] = indexData.PopulationType[keyName]._FAR[1].ToString();
            //    insertValue[i][6] = indexData.PopulationType[keyName]._maxDensity.ToString();
            //    insertValue[i][7] = indexData.PopulationType[keyName]._minGreen.ToString();
            //    insertValue[i][8] = indexData.PopulationType[keyName]._maxHeight.ToString();
            //    insertValue[i][9] = FunctionName["R"].ToString();
            //}
            //DB_Manager.InsertData(connectionString, sql, tablename, attrName, insertValue);
            #endregion

            #region 读取数据库数据 EC/WC/GC
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlab_db";
            //int attrCount = 6;
            //string sql = "select cv.cv_id, cv.cv_name, bf.name, bf.relative_name, cv.cv_max, cv.cv_min " +
            //    "from building_consumption_value cv, building_functions bf " +
            //    "where cv.building_type = bf.id;";

            //var resultList = DB_Manager.GetData(connectionString, sql, attrCount);
            #endregion

            #region 读取数据库数据 Population
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlab_db";
            //int attrCount = 12;
            //string sql = "select " +
            //    "bp.bp_id, bp.bp_name, bp.bp_layer_min,bp.bp_layer_max, bp.bp_people, bp.bp_far_min, bp.bp_far_max," +
            //    "bp.bp_density_max,bp.bp_green_min,bp.bp_height_max,bf.name, bf.relative_name " +
            //    "from " +
            //    "building_population bp, building_functions bf " +
            //    "where" +
            //    " bp.bp_func_id = bf.id;";

            //var resultList = DB_Manager.GetData(connectionString, sql, attrCount);
            #endregion

            #region 读取数据库数据 成本库
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlab_db";
            //string city = "北京";
            //int attrCount = 9;
            //string sql = string.Format("select " +
            //    "bf.name,qf.quality_name,cc.quality_id,c.name, cc.price_max, cc.price_min,cc.year,c.lat, c.lon " +
            //    "FROM construction_cost cc, cities c, building_functions bf, quality_functions qf " +
            //    "where cc.city_id = c.code and cc.func_id = bf.id and cc.quality_id = qf.quality_id and c.name='{0}'; ", city);

            //var resultList = DB_Manager.GetData(connectionString, sql, attrCount);
            //FuncionClass cityInfo = new FuncionClass(resultList);
            //var result = cityInfo.FuncInfoDic;

            //Console.WriteLine("城市为{0}, 经度为{1},纬度{2}", cityInfo.CityName, cityInfo.Lat.ToString(), cityInfo.Lon.ToString());
            #endregion

            #region 读取geojson
            //var jsonFilePath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geojson\building.geojson";
            //StreamReader sr = File.OpenText(jsonFilePath);

            //var feactureCollection = GeoJsonReader.GetFeatureCollectionFromJson(sr.ReadToEnd());

            //for (int i = 0; i < feactureCollection.Count; i++)
            //{
            //    var jsonDic = feactureCollection[i].Attributes["function"];
            //    Console.WriteLine(jsonDic.ToString());
            //}
            #endregion

            #region 测试计算整体内容
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlab_db";
            //string city = "北京";
            //string jsonFile = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geojson\building.geojson";
            //double hqratio = 0.5;

            //Sustainable_calculation calc = new Sustainable_calculation(connectionString, city, jsonFile, hqratio);

            //Console.WriteLine("建设成本为{0}, 人口为{1},垃圾量为{2}", calc.Result.ConstructionCost,calc.Result.Population,calc.Result.WConsumption);
            #endregion

            #region 计算行道树CO2吸收量
            //string connectionString = "Host=39.107.177.223;Username=postgres;Password=admin;Database=urbanxlab_db";
            //string treeName = "毛白杨";
            //var DBH = new List<double>() { 6d, 12d };

            //var calc = new TreeCO2_calculation(connectionString, treeName, DBH);
            //Console.WriteLine("树木体积为{0}m³, Biomass为{2}kg, 总吸收二氧化碳量为{2}kg", calc.totalVolume, calc.totalDWB, calc.totalCO2);
            #endregion

            #region 测试自动生成ppt
            //string strPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\测试ppt.pptx";
            //string savedPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\PPT_Test";
            //Dictionary<string, string> keyValues = new Dictionary<string, string>();
            //keyValues.Add("idField", "101");
            //keyValues.Add("titleField", "测试101");
            //keyValues.Add("nameField", "郑同学");
            //keyValues.Add("funField", "");
            //keyValues.Add("proField", "");
            //keyValues.Add("sproField", "");

            //var path=PowerPointHelper.ReplacePowerPoint(strPath, savedPath, keyValues);
            #endregion

            #endregion

            #region 其他测试
            #region 001_根据节点，读取xml数据
            //string readPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\kmlTest\test001.kml";
            //string savePath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\kmlTest\testOutput_4.kml";
            //Point pointMin = new Point(110.9880, 36.7666);
            //Point pointMax = new Point(113.7515, 38.7294);

            //ToolManagers.CreateKMLFile(readPath, savePath, pointMin, pointMax, 150, 150);
            //Console.WriteLine("完成");
            #endregion

            #region 002_输出分布函数
            //List<double> resultList = new List<double>();
            //int count = 100;
            //var result = StatisticsModel.NormalDistribution(10, 0.1, count, 1);

            //foreach (var item in resultList)
            //    Console.WriteLine(item);
            #endregion

            #region 003_地理位置编码
            //var data = GaodeLocation.DecodeResult("成都");
            //Console.WriteLine(data.adcode +"\n"+data.latitude+ "\n"+data.lontitude);
            #endregion

            #region 004_批量上传shp数据进数据库
            //string host = "39.107.177.223";
            //string user = "postgres";
            //string pwd = "admin";
            //string db = "urbanxlab_db";
            //string schema = "geometry_data";
            //string table = "beijing";


            //string shpfile = @"E:\114_temp\014_data\geojson\北京市.shp";
            //var result=DB_Manager.OutputSHPtoPSGL(shpfile, host, user, pwd, db, schema, true, table);
            //Console.WriteLine(result);
            #endregion

            #region 005_读取CSV
            ////读取
            //DateTime start = System.DateTime.Now;
            //string filePath = @"E:\114_temp\015_DEMData\tets003\CASER\force\QGIS_Pt_Cut1.xyz";

            //using var streamReader = File.OpenText(filePath);
            //using var csvReader = new CsvReader(streamReader, CultureInfo.CurrentCulture);

            //Dictionary<string, List<int>> csvDic = new Dictionary<string, List<int>>();

            //while (csvReader.Read())
            //{
            //    var readingString = csvReader.GetField(0).Split(" ");
            //    var y = readingString[1];
            //    var z = int.Parse(readingString[2]);

            //    //var y = csvReader.GetField(1);
            //    //var z = int.Parse(csvReader.GetField(2));

            //    if (csvDic.ContainsKey(y))
            //        csvDic[y].Add(z);
            //    else
            //        csvDic.Add(y, new List<int>() {z});
            //}
            //ToolManagers.TimeCalculation(start, "读取并整理结束");

            ////写入
            //string exportfilePath = @"E:\114_temp\015_DEMData\tets003\CASER\force\QGIS_Pt_output2.csv";
            //var columnsLength = csvDic.ElementAt(0).Value.Count;
            //var rowLength = csvDic.Keys.Count;

            //Console.WriteLine("columnsLength : {0}\nrowLength : {1} ", columnsLength, rowLength);

            //DataTable dt = new DataTable("NewDt");

            ////创建其它列表
            //for (int i = 0; i < columnsLength; i++)
            //{
            //    dt.Columns.Add(new DataColumn($"{i}", Type.GetType("System.Int32")));
            //}
            //ToolManagers.TimeCalculation(start, "建立空表结束");

            //for (int i = 0; i < rowLength; i++)
            //{
            //    var singleList = csvDic.ElementAt(i).Value;
            //    DataRow dr = dt.NewRow();
            //    for (int j = 0; j < singleList.Count; j++)
            //    {
            //        dr[j] = singleList[j];
            //    }
            //    dt.Rows.Add(dr);
            //}
            //ToolManagers.TimeCalculation(start, "建表结束");

            //SaveCSV(dt, exportfilePath);

            #endregion

            #region 006_创建多种分布函数
            //var mean = 10d;
            //var min = -10d;
            //var max = 40d;
            //var count = 20;
            //var seed = DateTime.Now.Second;
            //var result = StatisticsModel.NormalDistribution(mean, min, max, count, seed);

            //foreach (var item in result)
            //{
            //    Console.WriteLine(item);
            //}

            //Console.WriteLine("Mean = {0}\nSum = {1}", result.Average(), result.Sum());
            //Console.ReadLine();
            #endregion
            #endregion


            #region 三维计算
            #region 000_练习
            #region 001_Mesh Creation
            //var vertices = new List<Vector3f>() { new Vector3f(0,0,0), new Vector3f(10,0,0), new Vector3f(0,10,0) } ;
            //var triangles = new int[] {0,1,2};
            //var normals= new List<Vector3f>() { new Vector3f(0,0,1), new Vector3f(0, 0, 1), new Vector3f(0, 0, 1) };

            //MeshCreation.CreateMesh(vertices,triangles,normals,out DMesh3 meshResult);
            //Console.WriteLine(meshResult.EdgeCount);
            #endregion

            #region 002_Basic Mesh File I/O
            //string filePath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geometryTest\export.obj";
            //string message=MeshCreation.ExportMesh(filePath, meshResult);
            //Console.WriteLine(message);
            #endregion
            #endregion

            #region 001_测试射线
            //var jsonFilePath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geometryTest\building_center.geojson";
            ////var jsonFilePath = @"C:\Users\CAUPD-BJ141\Desktop\西安建筑基底_32650.geojson";
            //string exportPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geometryTest\export_collection_center.obj";

            ////读取mesh
            //var inputDataCollection = MeshCreation.ReadJsonData(jsonFilePath, "floors", out double[] heightCollection);
            //ToolManagers.TimeCalculation(start, "读取");

            ////创建mesh simple
            //var extrudedMeshSimple = MeshCreation.ExtrudeMeshFromPt(inputDataCollection, heightCollection);
            //ToolManagers.TimeCalculation(start, "成面");

            //输出原始Mesh
            //string exportPath01 = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geometryTest\export_simple.stl";
            //MeshCreation.ExportMeshAsStl(exportPath01, extrudedMeshSimple, false);
            //ToolManagers.TimeCalculation(start, "输出模型");


            //////创建mesh
            ////var extrudedMesh = MeshCreation.ExtrudeRemeshMeshFromPt(inputDataCollection, heightCollection, 10, 0.5);
            ////ToolManagers.TimeCalculation(start, "成面+细分");

            //////输出细分Mesh
            ////MeshCreation.ExportMeshAsObj(exportPath, extrudedMesh, false);
            ////ToolManagers.TimeCalculation(start, "输出模型");


            ////加载细分后模型
            //var importPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geometryTest\export_collection_center.obj";
            //var loadedMesh = MeshCreation.ImportMesh(importPath);
            //ToolManagers.TimeCalculation(start, "加载模型");

            ////初始化颜色
            //MeshCreation.InitiateColor(loadedMesh);

            ////计算射线
            ////var ptOrigin = new Vector3d[] { 
            ////    new Vector3d(448222.34214,4410631.793928,0),
            ////    new Vector3d(448084.822782,4410256.155238,0),
            ////    new Vector3d(448557.527164,4410000.52232,0),
            ////    new Vector3d(448548.077951,4410634.381701,0),
            ////    new Vector3d(447829.673727,4410882.658433,0),
            ////};

            //var ptOrigin = new Vector3d[] {
            //    new Vector3d(0,0,0),
            //    new Vector3d(1,1,0),
            //    new Vector3d(2,2,0),
            //    new Vector3d(3,3,0),
            //    new Vector3d(447829.673727,4410882.658433,0),
            //};
            //var count = 1;
            //var ptLargeList = new List<Vector3d>(count * 5);
            //for (int i = 0; i < count; i++)
            //{
            //    ptLargeList.AddRange(ptOrigin.ToList());
            //}
            //ToolManagers.TimeCalculation(start, "初始化数据");


            //var rayResultDic = MeshCreation.CalcRays(loadedMesh, ptLargeList, 10, 100, 360, 200, 14);
            //var meshFromRays = MeshCreation.ApplyColorsBasedOnRays(loadedMesh, rayResultDic, Colorf.White, Colorf.Red);
            //ToolManagers.TimeCalculation(start, "计算射线");

            ////输出计算后Mesh
            //var exportPath_Calc = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geometryTest\export_calc.stl";
            //MeshCreation.ExportMeshAsStl(exportPath_Calc, meshFromRays, true);
            //ToolManagers.TimeCalculation(start, "输出计算后模型");
            #endregion

            #region 002_计算射线指标
            //var jsonFilePath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\building_center.geojson";
            //////var jsonFilePath = @"C:\Users\CAUPD-BJ141\Desktop\西安建筑基底_32650.geojson";
            //string exportPath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\export_collection_center_remesh.obj";

            //////读取mesh
            //var inputDataCollection = MeshCreation.ReadJsonData(jsonFilePath, "baseHeight", "brepHeight", out double[] heightCollection);
            //ToolManagers.TimeCalculation(start, "读取");

            //////创建mesh simple，输出中心点与面积

            //var simpleMesh = MeshCreation.ExtrudeMeshFromPtMinusTopBtn(inputDataCollection, heightCollection, out Dictionary<NetTopologySuite.Geometries.Point, double> secPtDic);
            ////ToolManagers.TimeCalculation(start, "挤出mesh并输出数据");

            ////初始化可视点数据
            //var ptOrigin = new List<NetTopologySuite.Geometries.Point>() {
            //    new  NetTopologySuite.Geometries.Point(0,0),
            //    new  NetTopologySuite.Geometries.Point(-100,-100),
            //    new  NetTopologySuite.Geometries.Point(-200,-200),
            //    new  NetTopologySuite.Geometries.Point(-300,-300),
            //    new  NetTopologySuite.Geometries.Point(-400,-400),
            //};
            //var count = 1;
            //var ptLargeList = new List<NetTopologySuite.Geometries.Point>(count * ptOrigin.Count);

            //for (int i = 0; i < count; i++)
            //    ptLargeList.AddRange(ptOrigin);
            //ToolManagers.TimeCalculation(start, "初始化数据");

            ////开始计算可视范围内的点，及所包含建筑的总面积（去除顶面和底面）
            //var wholePtList = Poly2DCreation.ContainsInPts(ptLargeList.ToArray(), secPtDic.Keys.ToArray(), 300);
            //var wholeAreaList = Poly2DCreation.ContainsAreaInPts(wholePtList, secPtDic);
            //ToolManagers.TimeCalculation(start, "计算范围内总面积");

            ////创建细分mesh
            //var remeshedMesh = MeshCreation.SimpleRemesher(simpleMesh, 10, 0.5);
            //ToolManagers.TimeCalculation(start, "细分");

            //////输出细分Mesh
            //MeshCreation.ExportMeshAsObj(exportPath, remeshedMesh, false);
            //ToolManagers.TimeCalculation(start, "输出模型");


            ////加载细分后模型
            //var importPath = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geometryTest\export_collection_center_remesh.obj";
            //var loadedMesh = MeshCreation.ImportMesh(importPath);
            //ToolManagers.TimeCalculation(start, "加载模型");

            ////计算射线及比例，按照largeList顺序
            //var visibleAreaList = MeshCreation.CalcRaysGetArea(remeshedMesh, MeshCreation.NTSPtList2Vector3dList(ptLargeList), 10, 100, 360, 200, 14);
            //ToolManagers.TimeCalculation(start, "计算射线");

            //var visibilityPercentage = MeshCreation.CalcVisibilityPercent(visibleAreaList, wholeAreaList);
            //ToolManagers.TimeCalculation(start, "计算可视总面积");



            //////输出计算后Mesh
            ////var exportPath_Calc = @"E:\114_temp\008_代码集\002_extras\smallCharpTool\Application\data\geometryTest\export_calc.stl";
            ////MeshCreation.ExportMeshAsStl(exportPath_Calc, meshFromRays, true);
            ////ToolManagers.TimeCalculation(start, "输出计算后模型");

            #endregion

            #region 003_计算射线指标_基于
            #endregion

            #region 004_SketchUp测试

            #region 读取SU模型
            //var jsonFilePath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\building_center02.geojson";
            //var exportPath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\building_center_2_2013.skp";
            //var loadedPath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\loadedModel_2.skp";

            //var inputDataCollection = MeshCreation.ReadJsonData(jsonFilePath, "baseHeight", "brepHeight", out double[] heightCollection, out double[][] envelopeCollection);
            //ToolManagers.TimeCalculation(start, "读取");

            //var result=SketchUpManager.ExtrudeSUModelFromData(inputDataCollection,heightCollection,envelopeCollection);
            //ToolManagers.TimeCalculation(start, "创建skp模型");

            //var meshLoaded=SketchUpManager.LoadFromSkp(loadedPath);

            ////SketchUpManager.WriteSUModel(result, exportPath,"2020");
            //SketchUpManager.AppendToSUModel(result, exportPath,"2013");
            //ToolManagers.TimeCalculation(start, "存储skp模型");

            #endregion
            #endregion

            #region 005_日照计算
            //var test = new SunCalculator(39.904030, 116.407526, 10, 2018,12,23,12,0,8);
            //Console.WriteLine($"{test.SunDistance}");

            #endregion

            #region 006_计算射线_从三角面出发_多线程
            //var jsonFilePath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\building_center.geojson";
            //string exportPath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\export_collection_center_remesh.obj";
            //string tempExportPath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\temp_data.geojson";

            //#region 初始化模型，并输出对应数据_中心点、面积、细分后mesh
            //////读取mesh
            ////var inputDataCollection = MeshCreation.ReadJsonData(jsonFilePath, "baseHeight", "brepHeight", out double[] heightCollection);
            ////ToolManagers.TimeCalculation(start, "读取");

            ////////创建mesh simple，输出中心点与面积

            ////var simpleMesh = MeshCreation.ExtrudeMeshFromPtMinusTopBtn(inputDataCollection, heightCollection, out Dictionary<NetTopologySuite.Geometries.Point, double> secPtDic);
            ////var fc = MeshCreation.BuildFeatureCollection(secPtDic.Keys.ToArray(), secPtDic.Values.ToArray());
            ////MeshCreation.ExportGeoJSON(fc, tempExportPath);

            //////创建细分mesh
            ////var remeshedMesh = MeshCreation.SimpleRemesher(simpleMesh, 10, 0.5);
            ////ToolManagers.TimeCalculation(start, "细分");

            ////////输出细分Mesh
            ////MeshCreation.ExportMeshAsObj(exportPath, remeshedMesh, false);
            ////ToolManagers.TimeCalculation(start, "输出模型");
            //#endregion


            //////初始化可视点数据
            //var pointArray = MeshCreation.ReadJsonData(tempExportPath, "Area", out double[] areaCollection);
            //var ptOrigin = new List<NetTopologySuite.Geometries.Point>() {
            //    new  NetTopologySuite.Geometries.Point(0,0,0),
            //    //new  NetTopologySuite.Geometries.Point(100,100,0),
            //    //new  NetTopologySuite.Geometries.Point(200,200,0),
            //    //new  NetTopologySuite.Geometries.Point(300,300,0),
            //    //new  NetTopologySuite.Geometries.Point(400,400,0),
            //};
            //var count = 1;
            //var ptLargeList = new List<NetTopologySuite.Geometries.Point>(count * ptOrigin.Count);

            //for (int i = 0; i < count; i++)
            //    ptLargeList.AddRange(ptOrigin);
            //ToolManagers.TimeCalculation(start, "初始化数据");

            //////开始计算可视范围内的点，及所包含建筑的总面积（去除顶面和底面）
            ////var wholePtList = Poly2DCreation.ContainsInPts(ptLargeList.ToArray(), pointArray, 300);
            ////var wholeAreaList = Poly2DCreation.ContainsAreaInPts(wholePtList, secPtDic);
            ////ToolManagers.TimeCalculation(start, "计算范围内总面积");


            ////加载细分后模型
            //var importPath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\export_collection_center_remesh.obj";
            //var loadedMesh = MeshCreation.ImportMesh(importPath);
            //ToolManagers.TimeCalculation(start, "加载模型");

            //////初始化颜色
            //MeshCreation.InitiateColor(loadedMesh);

            ////开始相切
            //MeshCreation.GetTri(loadedMesh, ptLargeList.ToArray(),600, out Dictionary<int, int> meshIntrDic);


            //////计算射线及比例，按照largeList顺序
            ////var visibleAreaList = MeshCreation.CalcRaysGetArea(remeshedMesh, MeshCreation.NTSPtList2Vector3dList_2d(ptLargeList), 10, 100, 360, 200, 14);
            ////ToolManagers.TimeCalculation(start, "计算射线");

            ////var visibilityPercentage = MeshCreation.CalcVisibilityPercent(visibleAreaList, wholeAreaList);
            ////ToolManagers.TimeCalculation(start, "计算可视总面积");

            ////上颜色
            //var meshFromRays = MeshCreation.ApplyColorsBasedOnRays(loadedMesh, meshIntrDic, Colorf.White, Colorf.Red);
            //ToolManagers.TimeCalculation(start, "计算射线");

            ////输出计算后Mesh
            //var exportPath_Calc = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\export_calc_20210427.obj";
            //MeshCreation.ExportMeshAsObj(exportPath_Calc, meshFromRays, true);
            ////ToolManagers.TimeCalculation(start, "输出计算后模型");
            #endregion

            #region 007_计算射线_PlanktonMesh
            var jsonFilePath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\building_center02.geojson";
            string exportPath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\export_collection_center_remesh.obj";
            string exportPathSmallBox = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\export_collection_center_smallBox.obj";
            string tempExportPath = @"E:\114_temp\008_代码集\002_extras\smallCSharpAddtion\Application\data\geometryTest\temp_data02.geojson";

            #region 初始化模型，并输出对应数据_中心点、面积、细分后mesh
            //读取mesh
            var inputDataCollection = MeshCreation.ReadJsonData(jsonFilePath, "baseHeight", "brepHeight", out double[] heightCollection);
            ToolManagers.TimeCalculation(start, "读取");

            ////创建mesh simple，输出中心点与面积

            var simpleMesh = MeshCreation.ExtrudeMeshFromPtMinusTopBtn(inputDataCollection, heightCollection, out Dictionary<NetTopologySuite.Geometries.Point, double> secPtDic);
            //var simpleMesh = MeshCreation.BoundarySrfFromPts(inputDataCollection[0]);
            //var fc = MeshCreation.BuildFeatureCollection(secPtDic.Keys.ToArray(), secPtDic.Values.ToArray());
            //MeshCreation.ExportGeoJSON(fc, tempExportPath);
            MeshCreation.ExportMeshAsObj(exportPathSmallBox, simpleMesh);

            var Anchors = new List<Vector3d>();
            var SetCreases = new List<DCurve3>();
            var TargetCreases = new List<DCurve3>();
            var HardCreases = new List<bool>();
            //创建细分mesh
            var pMesh = simpleMesh.g3Mesh2pMesh();
            //var remeshedMesh = MeshCreation.ReMesh(0, pMesh, 1, Anchors, SetCreases, TargetCreases,HardCreases);
            ToolManagers.TimeCalculation(start, "细分");

            ////输出细分Mesh
            var exportedMesh = pMesh.pMesh2g3Mesh();
            MeshCreation.ExportMeshAsObj(exportPath, exportedMesh, false);
            ToolManagers.TimeCalculation(start, "输出模型");
            #endregion

            #region pk测试
            #endregion 


            #endregion

            #endregion
            ToolManagers.TimeCalculation(start, "完成");
            Console.ReadLine();
        }

        public static void SaveCSV(DataTable dt, string fullPath)//table数据写入csv
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(fullPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            System.IO.FileStream fs = new System.IO.FileStream(fullPath, System.IO.FileMode.Create,
                System.IO.FileAccess.Write);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
            string data = "";

            for (int i = 0; i < dt.Columns.Count; i++)//写入列名
            {
                data += dt.Columns[i].ColumnName.ToString();
                if (i < dt.Columns.Count - 1)
                {
                    data += ",";
                }
            }
            sw.WriteLine(data);

            for (int i = 0; i < dt.Rows.Count; i++) //写入各行数据
            {
                data = "";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string str = dt.Rows[i][j].ToString();
                    str = str.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号
                    if (str.Contains(',') || str.Contains('"')
                        || str.Contains('\r') || str.Contains('\n')) //含逗号 冒号 换行符的需要放到引号中
                    {
                        str = string.Format("\"{0}\"", str);
                    }

                    data += str;
                    if (j < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
            }
            sw.Close();
            fs.Close();
        }



    }
}

