using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UrbanX.Application
{
    public class GaodeLocation
    {
        private static readonly string _key = "34b0bbcfc3236cee8a53fdf28ba768ec";
        private static readonly string _base = "https://restapi.amap.com/v3/geocode/geo?address=";

        public GeocodeResult cityInfo;

        private static string Geocode(string address)
        {
            string strjson;
            try
            {
                string URL = _base + address + "&output=JSON&key=" + _key;
                string strBuff = "";
                int byteRead = 0;
                char[] cbuffer = new char[256];
                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(URL));
                HttpWebResponse httpResp = (HttpWebResponse)httpReq.GetResponse();
                Stream respStream = httpResp.GetResponseStream();
                StreamReader respStreamReader = new StreamReader(respStream, System.Text.Encoding.UTF8);
                byteRead = respStreamReader.Read(cbuffer, 0, 256);
                while (byteRead!=0)
                {
                    string strResp = new string(cbuffer, 0, byteRead);
                    strBuff = strBuff + strResp;
                    byteRead = respStreamReader.Read(cbuffer, 0, 256);
                }
                respStream.Close();
                strjson = strBuff;
                return strjson;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static GeocodeResult ResultParse(string strjson)
        {
            JObject jo = JObject.Parse(strjson);
            string geocodes = jo["geocodes"].ToString();
            JArray o = JArray.Parse(geocodes);
            string index = o[0].ToString();
            JObject o2 = JObject.Parse(index);

            string status = jo["status"].ToString();
            string count = jo["count"].ToString();
            string info = jo["info"].ToString();

            string formatted_address= o2["formatted_address"].ToString();// 结构化地址信息
            string province = o2["province"].ToString(); ;// 所在省
            string city = o2["city"].ToString(); ;// 城市
            string citycode = o2["citycode"].ToString(); ;// 城市编码
            string district = o2["district"].ToString(); ;// 地址所在的区
            string township = o2["township"].ToString(); ;// 地址所在的乡镇
            string street = o2["street"].ToString(); ;// 街道
            string number = o2["number"].ToString(); ;// 门牌
            string adcode = o2["adcode"].ToString(); ;// 区域编码
            double latitude=double.Parse(o2["location"].ToString().Split(',')[1]);// 纬度
            double lontitude= double.Parse(o2["location"].ToString().Split(',')[0]);// 经度
            string level = o2["level"].ToString(); ;// 匹配级别

            var result = new GeocodeResult(status,count,info,
                formatted_address, province, city, citycode,
                district, township, street, number, adcode,
                latitude, lontitude, level
                );

            return result;
    }

        public static GeocodeResult DecodeResult(string address)
        {
            var cityInfo = ResultParse(Geocode(address));
            return cityInfo;
        }
    }
}
