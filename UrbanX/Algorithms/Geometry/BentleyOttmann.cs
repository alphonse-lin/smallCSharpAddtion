using System;
using System.Collections.Generic;
using System.Linq;

using UrbanX.DataStructures.Geometry;
using UrbanX.DataStructures.Heaps;
using UrbanX.DataStructures.Trees;

namespace UrbanX.Algorithms.Geometry
{
    /// <summary>
    /// Unfortunately, this algorithm has some issues, Please try Brute force later.
    /// </summary>


    internal enum EventType
    {
        StartPoint = 0,
        InterPoint = 1,
        EndPoint = 2
    }

    internal interface IPointEvent : IComparable<IPointEvent>, IEquatable<IPointEvent>
    {
        EventType Type { get; }
        Point Point { get; }

        Segment SegmentA { get; }
        Segment SegmentB { get; }
    }


    internal struct StartEvent : IPointEvent
    {
        public EventType Type { get; }

        public Point Point { get; }
        // The index represent the line in a container.
        // For start and end point.
        public Segment SegmentA { get; }

        public Segment SegmentB { get; }

        public StartEvent(EventType type, Point point, Segment segment)
        {
            Type = type;
            Point = point;
            SegmentA = segment;
            SegmentB = segment;

        }

        /// <summary>
        /// For event comparer, there need three steps for BinaryMinHeap.
        /// 1. x ; 2. y ; 3. the order of insertion.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(IPointEvent other)
        {
            var c = this.Point.X.CompareTo(other.Point.X);
            return c == 0 ? this.Point.Y.CompareTo(other.Point.Y) : c;
        }

        public bool Equals(IPointEvent other)
        {
            if (other == null)
                return false;

            return this.Type == other.Type && this.Point.Equals(other.Point) && this.SegmentA.Equals(other.SegmentA) && this.SegmentB.Equals(other.SegmentB);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StartEvent))
                return false;

            StartEvent s = (StartEvent)obj;
            return this.Equals(s);
        }

        public override int GetHashCode()
        {
            // For using Hashtale
            // MSDN docs recommend XOR'ing the internal values to get a hash code
            return this.Type.GetHashCode() ^ this.Point.GetHashCode() ^ this.SegmentA.GetHashCode() ^ this.SegmentB.GetHashCode();
        }
    }

    internal struct EndEvent : IPointEvent
    {
        public EventType Type { get; }

        public Point Point { get; }
        // The index represent the line in a container.
        // For start and end point.
        public Segment SegmentA { get; }
        public Segment SegmentB { get; }

        public EndEvent(EventType type, Point point, Segment segment)
        {
            Type = type;
            Point = point;
            SegmentA = segment;
            SegmentB = segment;
        }

        /// <summary>
        /// For event comparer, there need three steps for BinaryMinHeap.
        /// 1. x ; 2. y ; 3. the order of insertion.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(IPointEvent other)
        {
            var c = this.Point.X.CompareTo(other.Point.X);
            return c == 0 ? this.Point.Y.CompareTo(other.Point.Y) : c;
        }


        public bool Equals(IPointEvent other)
        {
            if (other == null)
                return false;

            return this.Type == other.Type && this.Point.Equals(other.Point) && this.SegmentA.Equals(other.SegmentA) && this.SegmentB.Equals(other.SegmentB);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EndEvent))
                return false;

            EndEvent e = (EndEvent)obj;
            return this.Equals(e);
        }

        public override int GetHashCode()
        {
            // For using Hashtale
            // MSDN docs recommend XOR'ing the internal values to get a hash code
            return this.Type.GetHashCode() ^ this.Point.GetHashCode() ^ this.SegmentA.GetHashCode() ^ this.SegmentB.GetHashCode();
        }
    }

    internal struct IntersectionEvent : IPointEvent
    {
        public EventType Type { get; }

        public Point Point { get; }
        // The index represent the line in a container.
        // For start and end point.
        public Segment SegmentA { get; }
        public Segment SegmentB { get; }

        public IntersectionEvent(EventType type, Point point, Segment segmentA, Segment segmentB)
        {
            Type = type;
            Point = point;
            SegmentA = segmentA;
            SegmentB = segmentB;
        }

        /// <summary>
        /// For event comparer, there need three steps for BinaryMinHeap.
        /// 1. x ; 2. y ; 3. the order of insertion.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(IPointEvent other)
        {
            var c = this.Point.X.CompareTo(other.Point.X);
            return c == 0 ? this.Point.Y.CompareTo(other.Point.Y) : c;
        }

        public bool Equals(IPointEvent other)
        {
            if (other == null)
                return false;

            return this.Type == other.Type && this.Point.Equals(other.Point) && ((this.SegmentA.Equals(other.SegmentA) && this.SegmentB.Equals(other.SegmentB)) || (this.SegmentA.Equals(other.SegmentB) && this.SegmentB.Equals(other.SegmentA)));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IntersectionEvent))
                return false;

            IntersectionEvent i = (IntersectionEvent)obj;
            return this.Equals(i);
        }

        public override int GetHashCode()
        {
            // For using Hashtale
            // MSDN docs recommend XOR'ing the internal values to get a hash code
            return this.Type.GetHashCode() ^ this.Point.GetHashCode() ^ this.SegmentA.GetHashCode() ^ this.SegmentB.GetHashCode();
        }
    }



    internal class Segment : IComparable<Segment>, IEquatable<Segment>
    {

        public Line Line { get; }
        public double Y { get; private set; }

        public Segment(Line line)
        {

            if (line.From.X < line.To.X)
            {
                Line = new Line(line.From, line.To);
            }
            else if (line.From.X == line.To.X)
            {
                if (line.From.Y < line.To.Y)
                {
                    Line = new Line(line.From, line.To);
                }
                else
                {
                    Line = new Line(line.To, line.From);
                }
            }
            else
            {
                Line = new Line(line.To, line.From);
            }

            Y = Line.From.Y;
        }

        public void GetYCoordinate(double x)
        {
            double xRun = Line.To.X - Line.From.X;
            double yRun = Line.To.Y - Line.From.Y;

            if (xRun == 0 || yRun == 0)
            {
                // means this is a vertical line or horizontal line.
                Y += Line.From.Y;
            }

            // y = ax +b 
            double a = yRun / xRun;
            double b = Line.From.Y - a * Line.From.X;

            Y = a * x + b;
        }

        public void UpdateYCoordinate(double value)
        {
            Y = value;
        }

        /// <summary>
        /// For segment comparer, there need three steps in RBTree.
        /// 1. y; 2. x; 3. order of insertion.
        /// But event already handled the step 2 and 3,
        /// Therefore, we only need to compare y.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Segment other)
        {
            var c = this.Y.CompareTo(other.Y);

            // Segments may share the same start point.
            return c == 0 ? this.Line.GetLineSlope().CompareTo(other.Line.GetLineSlope()) : c;
        }

        public bool Equals(Segment other)
        {
            if (other == null)
                return false;
            return this.Line.Equals(other.Line);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Segment))
                return false;

            Segment s = (Segment)obj;
            return this.Equals(s);
        }

        public override int GetHashCode()
        {
            // For using Hashtale
            // MSDN docs recommend XOR'ing the internal values to get a hash code
            return this.Line.GetHashCode();
        }
    }


    public class BentleyOttmann
    {
        readonly double _tolerance;

        readonly Line[] _lines;
        readonly BinaryMinHeap<IPointEvent> _eventQueue;
        readonly HashSet<IPointEvent> _eventKey;
        readonly RBTree<Segment> _segmentsTree;
        readonly Dictionary<Line, SortedSet<Point>> _sectionPtsForLine;

        // Can be deleted later.
        public LinkedList<Point> Intersections { get; }

        public HashSet<Line> Segments { get; }

        public BentleyOttmann(Line[] lines, double tolerance)
        {
            _tolerance = tolerance;
            _lines = lines;

            // Order is a big problem.
            _eventQueue = new BinaryMinHeap<IPointEvent>(lines.Length * 3);
            _eventKey = new HashSet<IPointEvent>(lines.Length);
            _segmentsTree = new RBTree<Segment>(false, TraversalMode.InOrder);
            _sectionPtsForLine = new Dictionary<Line, SortedSet<Point>>(lines.Length);


            Intersections = new LinkedList<Point>();
            Segments = new HashSet<Line>();

            Initialize();
            Processing();
            RebuildSegments();

        }

        private void Initialize()
        {
            for (int i = 0; i < _lines.Length; i++)
            {
                var line = _lines[i];
                var seg = new Segment(line);
                _sectionPtsForLine.Add(seg.Line, new SortedSet<Point>());

                var startEvent = new StartEvent(EventType.StartPoint, seg.Line.From, seg);
                var endEvent = new EndEvent(EventType.EndPoint, seg.Line.To, seg);
                _eventQueue.Add(startEvent);
                _eventQueue.Add(endEvent);
            }
        }

        private void Processing()
        {
            while (!_eventQueue.IsEmpty)
            {
                var pEvent = _eventQueue.ExtractMin();

                switch (pEvent.Type)
                {
                    case EventType.StartPoint:
                        var currentSeg0 = pEvent.SegmentA;

                        // Reorder the segment tree by updating the Y-coordinate of the intserction with the sweep line.
                        if (_segmentsTree.Count > 0)
                        {
                            var segs = _segmentsTree.ToArray();
                            _segmentsTree.Clear();
                            for (int i = 0; i < segs.Length; i++)
                            {
                                var seg = segs[i];
                                seg.GetYCoordinate(pEvent.Point.X);
                                _segmentsTree.Insert(seg);
                            }
                        }
                        _segmentsTree.Insert(currentSeg0);

                        var preNode0 = _segmentsTree.FindFloor(currentSeg0);
                        var sucNode0 = _segmentsTree.FindCeiling(currentSeg0);
                        if (preNode0 != null)
                        {
                            CheckIntersection(preNode0.Value, currentSeg0);
                        }
                        if (sucNode0 != null)
                        {
                            CheckIntersection(currentSeg0, sucNode0.Value);
                        }
                        break;

                    case EventType.InterPoint:
                        var currentSeg1a = pEvent.SegmentA;
                        var currentSeg1b = pEvent.SegmentB;
                        var preNode1a = _segmentsTree.FindFloor(currentSeg1a);
                        var sucNode1b = _segmentsTree.FindCeiling(currentSeg1b);
                        if (preNode1a != null)
                        {
                            CheckIntersection(preNode1a.Value, currentSeg1b);
                        }
                        if (sucNode1b != null)
                        {
                            CheckIntersection(currentSeg1a, sucNode1b.Value);
                        }

                        SwapSegmentsInTree(currentSeg1a, currentSeg1b);

                        break;

                    case EventType.EndPoint:
                        var currentSeg2 = pEvent.SegmentA;
                        var preNode2 = _segmentsTree.FindFloor(currentSeg2);
                        var sucNode2 = _segmentsTree.FindCeiling(currentSeg2);
                        if (preNode2 != null && sucNode2 != null)
                        {
                            CheckIntersection(preNode2.Value, sucNode2.Value);
                        }

                        _segmentsTree.Remove(currentSeg2);
                        break;

                }
            }
        }

        private void RebuildSegments()
        {
            foreach (var item in _sectionPtsForLine)
            {
                var pts = item.Value.ToArray();

                if (pts.Length != 0)
                {
                    Line sLine = new Line(item.Key.From, pts[0]);
                    Line eLine = new Line(pts[pts.Length - 1], item.Key.To);
                    Segments.Add(sLine);
                    if (pts.Length > 1)
                    {
                        for (int i = 0; i < pts.Length - 1; i++)
                        {
                            Line temp = new Line(pts[i], pts[i + 1]);
                            Segments.Add(temp);
                        }
                    }
                    Segments.Add(eLine);
                }
                else
                {
                    Segments.Add(item.Key);
                }
            }
        }


        private void CheckIntersection(Segment seg1, Segment seg2)
        {
            var sect = new LineIntersection(seg1.Line, seg2.Line, _tolerance);
            if (sect.Intersection != null)
            {
                var pt = sect.Intersection[0];

                var interEvent = new IntersectionEvent(EventType.InterPoint, pt, seg1, seg2);

                if (_eventKey.Contains(interEvent))
                    return;

                _eventQueue.Add(interEvent);
                _eventKey.Add(interEvent);

                Intersections.AddLast(pt);
                _sectionPtsForLine[seg1.Line].Add(pt);
                _sectionPtsForLine[seg2.Line].Add(pt);
            }
        }

        private void SwapSegmentsInTree(Segment seg1, Segment seg2)
        {
            _segmentsTree.Remove(seg1);
            _segmentsTree.Remove(seg2);
            var tempY = seg1.Y;
            seg1.UpdateYCoordinate(seg2.Y);
            seg2.UpdateYCoordinate(tempY);

            _segmentsTree.Insert(seg1);
            _segmentsTree.Insert(seg2);
        }

    }

}
