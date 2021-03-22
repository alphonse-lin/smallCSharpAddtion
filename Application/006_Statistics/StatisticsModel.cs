using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace UrbanX.Application
{
    public class StatisticsModel
    {
        public static double[] NormalDistribution(double mean, double std, int count, int seed)
        {
            Random ran = new Random(seed);
            double[] resultArray = new double[count];

            MathNet.Numerics.Distributions.Normal test = new MathNet.Numerics.Distributions.Normal(mean, std);
            for (int i = 0; i < count; i++)
                resultArray[i] = test.Sample();
            return resultArray;
        }

        public static double[] NormalDistribution(double mean, double min, double max, int count, int seed)
        {
            Random ran = new Random(seed);
            double[] resultArray = new double[count];

            double std = (max - min) / 6;

            MathNet.Numerics.Distributions.Normal test = new MathNet.Numerics.Distributions.Normal(mean, std);
            for (int i = 0; i < count; i++)
                resultArray[i] = test.Sample();
            return resultArray;
        }

        public static double[] LogDistribution(double mean, double std, int count, int seed)
        {
            Random ran = new Random(seed);
            double[] resultArray = new double[count];

            MathNet.Numerics.Distributions.LogNormal test = new MathNet.Numerics.Distributions.LogNormal(mean, std);
            for (int i = 0; i < count; i++)
                resultArray[i] = test.Sample();
            return resultArray;
        }

        public static double Mean(IEnumerable<double> dataList)
        {
            return MathNet.Numerics.Statistics.Statistics.Mean(dataList);
        }
    }

}
