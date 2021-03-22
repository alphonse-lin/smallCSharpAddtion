using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UrbanX.DataStructures.Geometry
{
    public struct UrbanXPoint : IEquatable<UrbanXPoint>, IComparable<UrbanXPoint>
    {
        public double X { get; set; }
        public double Y { get; set; }

        public UrbanXPoint(double x, double y) => (X, Y) = (x, y);
        public UrbanXPoint(UrbanXVector point) { X = point.X; Y = point.Y; }

        public double DistanceTo(UrbanXPoint other)
        {
            return Math.Sqrt(Math.Pow(other.Y - this.Y, 2) + Math.Pow(other.X - this.X, 2));
        }

        public override string ToString() => $" [{X}, {Y}]";

        public bool Equals(UrbanXPoint other)
        {
            // For using contains method in collection.
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UrbanXPoint))
                return false;

            UrbanXPoint p = (UrbanXPoint)obj;
            return this.Equals(p);
        }

        public override int GetHashCode()
        {
            // For using Hashtale
            // MSDN docs recommend XOR'ing the internal values to get a hash code
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }

        public int CompareTo(UrbanXPoint other)
        {
            var c = this.X.CompareTo(other.X);
            return c == 0 ? this.Y.CompareTo(other.Y) : c;
        }
    }
    public struct UrbanXVector
    {
        public double X;
        public double Y;

        // Constructors.
        public UrbanXVector(double x, double y) { X = x; Y = y; }
        //public Vector() : this(double.NaN, double.NaN) { }

        public UrbanXVector(UrbanXPoint point) { X = point.X; Y = point.Y; }

        public static UrbanXVector operator -(UrbanXVector v, UrbanXVector w)
        {
            return new UrbanXVector(v.X - w.X, v.Y - w.Y);
        }

        public static UrbanXVector operator +(UrbanXVector v, UrbanXVector w)
        {
            return new UrbanXVector(v.X + w.X, v.Y + w.Y);
        }

        public static double operator *(UrbanXVector v, UrbanXVector w)
        {
            return v.X * w.X + v.Y * w.Y;
        }

        public static UrbanXVector operator *(UrbanXVector v, double mult)
        {
            return new UrbanXVector(v.X * mult, v.Y * mult);
        }

        public static UrbanXVector operator *(double mult, UrbanXVector v)
        {
            return new UrbanXVector(v.X * mult, v.Y * mult);
        }

        public double Cross(UrbanXVector v)
        {
            return X * v.Y - Y * v.X;
        }

        public override bool Equals(object obj)
        {
            var v = (UrbanXVector)obj;
            return (X - v.X).IsZero() && (Y - v.Y).IsZero();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public double DistanceTo(UrbanXVector other)
        {
            return Math.Sqrt(Math.Pow(other.Y - this.Y, 2) + Math.Pow(other.X - this.X, 2));
        }
    }
    public struct UrbanXLine : IEquatable<UrbanXLine>
    {
        public UrbanXPoint From { get; }
        public UrbanXPoint To { get; }

        public double Length { get { return From.DistanceTo(To); } }

        public UrbanXLine(UrbanXPoint start, UrbanXPoint end) => (From, To) = (start, end);
        public UrbanXLine(UrbanXVector start, UrbanXVector end)
        {
            From = new UrbanXPoint(start);
            To = new UrbanXPoint(end);
        } 

        public void GetBoundingBox(out double[] outMax, out double[] outMin)
        {
            double[] Max = new double[2];
            double[] Min = new double[2];
            var x1 = From.X;
            var x2 = To.X;
            var y1 = From.Y;
            var y2 = To.Y;

            if (x1 >= x2) { Max[0] = x1; Min[0] = x2; }
            else { Max[0] = x2; Min[0] = x1; }

            if (y1 >= y2) { Max[1] = y1; Min[1] = y2; }
            else { Max[1] = y2; Min[1] = y1; }

            outMax = Max;
            outMin = Min;
        }

        public double GetLineSlope()
        {
            var xRun = To.X - From.X;
            var yRun = To.Y - From.Y;

            if (xRun == 0)
                return double.PositiveInfinity;

            return yRun / xRun;
        }

        public override string ToString() => $"Line:{From}-->{To}";

        public bool Equals(UrbanXLine other)
        {
            return this.From.Equals(other.From) && this.To.Equals(other.To);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UrbanXLine))
                return false;

            UrbanXLine l = (UrbanXLine)obj;
            return this.Equals(l);
        }

        public override int GetHashCode()
        {
            // MSDN docs recommend XOR'ing the internal values to get a hash code
     
            return From.GetHashCode()^To.GetHashCode();
        }
    }
    public static class Extensions
    {
        private const double Epsilon = 1e-10;

        public static bool IsZero(this double d)
        {
            return Math.Abs(d) < Epsilon;
        }
    }

    }
