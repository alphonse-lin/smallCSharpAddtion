using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace utility
{
    public class ConstructionCostClass
    {
        private string _cityName;
        public string CityName
        {
            get { return _cityName; }
            set { _cityName = value; }
        }

        private string _cityID;

        public string CityID
        {
            get { return _cityID; }
            set { _cityID = value; }
        }

        private string[][] _office;
        public string[][] Office
        {
            get { return _office; }
            set { _office = value; }
        }

        private string[][] _shopping;
        public string[][] Shopping
        {
            get { return _shopping; }
            set { _shopping = value; }
        }

        private string[][] _hotel;
        public string[][] Hotel
        {
            get { return _hotel; }
            set { _hotel = value; }
        }

        private string[][] _residential_highrise;

        public string[][] Residential_HighRise
        {
            get { return _residential_highrise; }
            set { _residential_highrise = value; }
        }

        private string[][] _residential_house;

        public string[][] Residential_House
        {
            get { return _residential_house; }
            set { _residential_house = value; }
        }


        private string[][] _industrial;

        public string[][] Industrial
        {
            get { return _industrial; }
            set { _industrial = value; }
        }

        private string[][] _carpark;

        public string[][] Carpark
        {
            get { return _carpark; }
            set { _carpark = value; }
        }

        public ConstructionCostClass()
        {

        }
    }
}
