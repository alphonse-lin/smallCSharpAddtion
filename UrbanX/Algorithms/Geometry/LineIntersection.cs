using System;

using UrbanX.DataStructures.Geometry;


namespace UrbanX.Algorithms.Geometry
{
    /// <summary>
    /// Only for two dimentional lines.
    /// </summary>
    public class LineIntersection
    {
        readonly Point _p1, _p2, _q1, _q2;
        readonly double _tolerance;

        public Point[] Intersection { get; }

        public Line[] Segments { get; }

        /// <summary>
        /// Constructor of line intersection class.
        /// </summary>
        /// <param name="lp"></param>
        /// <param name="lq"></param>
        public LineIntersection(Line lp, Line lq, double tolerance)
        {
            (_p1, _p2) = (lp.From, lp.To);
            (_q1, _q2) = (lq.From, lq.To);
            _tolerance = tolerance / 1000;

            var o1 = Orientation(_p1, _p2, _q1);
            var o2 = Orientation(_p1, _p2, _q2);
            var o3 = Orientation(_q1, _q2, _p1);
            var o4 = Orientation(_q1, _q2, _p2);


            // Handle end points situations.
            if (o1 == 3 || o2 == 3 || o3 == 3 || o4 == 3)
                return;


            // General case.
            if (o1 != o2 && o3 != o4)
            {
                if (o1 == 0)
                {
                    Intersection = new Point[1] { _q1 };
                    Segments = new Line[3] { lq, new Line(_p1, _q1), new Line(_p2, _q1) };
                    return;
                }
                if (o2 == 0)
                {
                    Intersection = new Point[1] { _q2 };
                    Segments = new Line[3] { lq, new Line(_p1, _q2), new Line(_p2, _q2) };
                    return;
                }
                if (o3 == 0)
                {
                    Intersection = new Point[1] { _p1 };
                    Segments = new Line[3] { lp, new Line(_p1, _q2), new Line(_p1, _q1) };
                    return;
                }
                if (o4 == 0)
                {
                    Intersection = new Point[1] { _p2 };
                    Segments = new Line[3] { lp, new Line(_p2, _q2), new Line(_p2, _q1) };
                    return;
                }

                // find the intersection point.
                var pt = FindIntersection();
                Intersection = new Point[1] { pt };
                Segments = new Line[4];

                Point[] temp = { _p1, _p2, _q1, _q2 };
                for (int i = 0; i < temp.Length; i++)
                {
                    Segments[i] = new Line(pt, temp[i]);
                }
            }

            // Special cases.          
            if (o1 == 0 && o2 == 0 && o3 == 0 && o4 == 0)
            {
                // p1,q1,p2,q2
                if (OnSegment(_p1, _p2, _q1) && OnSegment(_q1, _q2, _p2))
                {
                    Intersection = new Point[2] { _q1, _p2 };
                    Segments = new Line[3] { new Line(_p1, _q1), new Line(_q1, _p2), new Line(_p2, _q2) };
                    return;
                }
                // p1,q2,p2,q1
                if (OnSegment(_p1, _p2, _q2) && OnSegment(_q1, _q2, _p2))
                {
                    Intersection = new Point[2] { _q2, _p2 };
                    Segments = new Line[3] { new Line(_p1, _q2), new Line(_q2, _p2), new Line(_p2, _q1) };
                    return;
                }
                // p2,q1,p1,q2
                if (OnSegment(_p1, _p2, _q1) && OnSegment(_q1, _q2, _p1))
                {
                    Intersection = new Point[2] { _q1, _p1 };
                    Segments = new Line[3] { new Line(_p2, _q1), new Line(_q1, _p1), new Line(_p1, _q2) };
                    return;
                }
                // p2,q2,p1,q1
                if (OnSegment(_p1, _p2, _q2) && OnSegment(_q1, _q2, _p1))
                {
                    Intersection = new Point[2] { _q2, _p1 };
                    Segments = new Line[3] { new Line(_p2, _q2), new Line(_q2, _p1), new Line(_p1, _q1) };
                    return;
                }
                // p1,q1,q2,p2
                if (OnSegment(_p1, _p2, _q1) && OnSegment(_p1, _p2, _q2))
                {
                    Intersection = new Point[2] { _q1, _q2 };
                    if (_p1.DistanceTo(_q1) < _p1.DistanceTo(_q2))
                    {
                        Segments = new Line[3] { new Line(_p1, _q1), lq, new Line(_q2, _p2) };
                        return;
                    }
                    else
                    {
                        Segments = new Line[3] { new Line(_p1, _q2), lq, new Line(_q1, _p2) };
                        return;
                    }
                }
                // q1,p1,p2,q2
                if (OnSegment(_q1, _q2, _p1) && OnSegment(_q1, _q2, _p2))
                {
                    Intersection = new Point[2] { _p1, _p2 };
                    if (_q1.DistanceTo(_p1) < _q1.DistanceTo(_p2))
                    {
                        Segments = new Line[3] { new Line(_q1, _p1), lp, new Line(_p2, _q2) };
                        return;
                    }
                    else
                    {
                        Segments = new Line[3] { new Line(_q1, _p2), lp, new Line(_p1, _q2) };
                        return;
                    }
                }
            }
        }



        /// <summary>
        /// To find orientation of ordered triplet.
        /// 0 --> Colinear
        /// 1 --> Clockwise
        /// 2 --> Counterclockwise
        /// 3 --> Share same end points
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private double Orientation(Point a, Point b, Point c)
        {

            Vector A = new Vector(c.X - b.X, c.Y - b.Y);
            Vector B = new Vector(c.X - a.X, c.Y - a.Y);

            if (A.Length == 0 || B.Length == 0)
                return 3;

            var val = (A.X * B.Y - A.Y * B.X) / (A.Length * B.Length);

            // colinear
            if (Math.Abs(val) <= _tolerance)
                return 0;
            // clock for 1 ; counterclock for 2
            return (val > 0) ? 1 : 2;
        }

        /// <summary>
        /// Given three colinear points a,b,c, checks if point c lies on line segemt ab.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool OnSegment(Point a, Point b, Point c)
        {
            if (c.X <= Math.Max(a.X, b.X) && c.X >= Math.Min(a.X, b.X) && c.Y <= Math.Max(a.Y, b.Y) && c.Y >= Math.Min(a.Y, b.Y))
                return true;

            return false;
        }


        private Point FindIntersection()
        {
            // Line p1p2 represented as a1x + b1y = c1
            double a1 = _p2.Y - _p1.Y;
            double b1 = _p1.X - _p2.X;
            double c1 = a1 * _p1.X + b1 * _p1.Y;

            // Line q1q2 represented as a2x + b2y = c2
            double a2 = _q2.Y - _q1.Y;
            double b2 = _q1.X - _q2.X;
            double c2 = a2 * _q1.X + b2 * _q1.Y;


            double determinant = a1 * b2 - a2 * b1;
            // Due to the precision of division, x and y will has a very little change.
            double x = (b2 * c1 - b1 * c2) / determinant;
            double y = (a1 * c2 - a2 * c1) / determinant;


            // Correct the coordinate for vertical and horizental lines.
            if (a1 == 0)
                y = _p1.Y;
            if (b1 == 0)
                x = _p1.X;
            if (a2 == 0)
                y = _q2.Y;
            if (b2 == 0)
                x = _q2.X;

            return new Point(x, y);

        }
    }

}
