using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace utility.DecodeAddress
{
    public class GeocodeResult
    {
        #region property + field
        public string formatted_address { get; }// 结构化地址信息
        public string province { get; }// 所在省
        public string city { get; }// 城市
        public string citycode { get; }// 城市编码
        public string district { get; }// 地址所在的区
        public string township { get; }// 地址所在的乡镇
        public string street { get; }// 街道
        public string number { get; }// 门牌
        public string adcode { get; }// 区域编码
        public double latitude { get; }// 纬度
        public double lontitude { get; }// 经度
        public string level { get; }// 匹配级别

        public string status { get; }// 结果状态0,表示失败,1:表示成功
        public string count { get;}// 返回结果的数目
        public string info { get;}//返回状态说明
        #endregion

        public GeocodeResult(string status, string count, string info, string formatted_address, string province, string city, string citycode, string district,
                                string township, string street, string number, string adcode, double latitude, double lontitude, string level)
        {
            this.status = status;
            this.count = count;
            this.info = info;
            
            this.formatted_address = formatted_address;
            this.province = province;
            this.city = city;
            this.citycode = citycode;
            this.district = district;
            this.township = township;
            this.street = street;
            this.number = number;
            this.adcode = adcode;
            this.latitude = latitude;
            this.lontitude = lontitude;
            this.level = level;
        }


        public string toString()
        {
        return "Geocodes [formatted_address=" + formatted_address + ", province=" + province + ", city=" + city
        + ", citycode=" + citycode + ", district=" + district + ", township=" + township + ", street=" + street
        + ", number=" + number + ", adcode=" + adcode + ",lat=" + latitude +",lon=" + lontitude + ", level=" + level + "]";
        }
    }
}
