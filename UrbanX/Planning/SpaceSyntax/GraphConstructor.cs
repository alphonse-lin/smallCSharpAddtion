using System;
using System.Collections.Generic;
using System.Linq;

using Rhino.Geometry;

using UrbanX.DataStructures.Graphs;

namespace UrbanX.Planning.SpaceSyntax
{
    /// <summary>
    /// Construct an undirected weighted graph for space syntax.
    /// Road segments are the vertices V in graph, their connections are the edge E in graph.
    /// <para>Two ways for edge weight. one is length , another is the angle between two segments.</para>
    /// </summary>
    public class GraphConstructor
    {
        // Tolerance for defining the equality of two points.
        private readonly double _tolerance;

        private readonly Curve[] _CurvesList;

        private readonly HashSet<Point3d> _pointsList;

        private readonly int[] _segmentVertices;

        private readonly Dictionary<Point3d, Stack<int>> _adjacentSegments;


        public UndirectedWeightedSparseGraph<int> GraphofMatricWeight { get; }
        public UndirectedWeightedSparseGraph<int> GraphofAngularWeight { get; }

        public GraphConstructor(Curve[] curves, double tolerance)
        {
            // The minimum tolerance should be 1E-8.
            _tolerance = tolerance * 1E-3 < 1E-8 ? 1E-8 : tolerance * 1E-3;

            PointEqualityComparer comparer = new PointEqualityComparer(_tolerance);

            // Curves should be already cleared during preparation stage.
            _CurvesList = curves;
            _pointsList = new HashSet<Point3d>(comparer);
            _segmentVertices = new int[_CurvesList.Length];

            _adjacentSegments = new Dictionary<Point3d, Stack<int>>(comparer);

            GraphofMatricWeight = new UndirectedWeightedSparseGraph<int>(_CurvesList.Length);
            GraphofAngularWeight = new UndirectedWeightedSparseGraph<int>(_CurvesList.Length);

            Initialize();
            FindAdjacentSegments();
            BuildingGraph();
        }


        private void Initialize()
        {
            // Add vertices of graph to collection. Using indices to represent the vertices instead of using objects itselves.
            // Objects(curves) can be queied later by using indices.
            for (int i = 0; i < _CurvesList.Length; i++)
            {
                _pointsList.Add(_CurvesList[i].PointAtStart);
                _pointsList.Add(_CurvesList[i].PointAtEnd);

                _segmentVertices[i] = i;
            }

            foreach (var pt in _pointsList)
            {
                _adjacentSegments.Add(pt, new Stack<int>());
            }
        }

        private void FindAdjacentSegments()
        {
            for (int i = 0; i < _CurvesList.Length; i++)
            {
                var segment = _CurvesList[i];
                Point3d[] endsPts = { segment.PointAtStart, segment.PointAtEnd };

                foreach (var pt in endsPts)
                {
                    if (_adjacentSegments.ContainsKey(pt))
                    {
                        _adjacentSegments[pt].Push(i);
                    }
                    else
                    {
                        // handle the tolerance error.
                        var needle = new Point3d[] { pt };
                        var cloestIndex = FindCloestPoints(_pointsList.ToArray(), needle);
                        var tempStartPt = _pointsList.ToArray()[cloestIndex[0]];
                        var distance = pt.DistanceTo(tempStartPt);
                        if (distance < _tolerance)
                        {
                            _adjacentSegments[tempStartPt].Push(i);
                        }
                    }
                }
            }
        }

        // For space syntax, we use undirected and weighted sparse graph.
        private void BuildingGraph()
        {
            GraphofMatricWeight.AddVertices(_segmentVertices);
            GraphofAngularWeight.AddVertices(_segmentVertices);

            // Add weighted edge in graph.
            foreach (var pt in _pointsList)
            {
                var stack = _adjacentSegments[pt];

                // If stack.count == 1 , Vertex is isolated, there is no need to add edge.
                while (stack.Count > 1)
                {
                    var v = stack.Pop();
                    foreach (var w in stack)
                    {
                        var lengthWeight = (_CurvesList[v].GetLength() + _CurvesList[w].GetLength()) * 0.5;
                        GraphofMatricWeight.AddEdge(v, w, lengthWeight);

                        // For angular weight.
                        // Current point is pt, current segment is v.
                        Vector3d v1, v2;
                        if (pt == _CurvesList[v].PointAtStart)
                        {
                            v1 = _CurvesList[v].TangentAtStart;
                            v1.Reverse();
                        }
                        else
                        {
                            v1 = _CurvesList[v].TangentAtEnd;
                        }

                        if (pt == _CurvesList[w].PointAtStart)
                        {
                            v2 = _CurvesList[w].TangentAtStart;
                        }
                        else
                        {
                            v2 = _CurvesList[w].TangentAtEnd;
                            v2.Reverse();
                        }

                        // Need to handle the tolerance, and weight can not be zero.
                        var angularWeight = Math.Round(GetAngleOfTwoVectors(v1, v2), 3);
                        if (angularWeight == 0)
                            angularWeight += 0.001;

                        GraphofAngularWeight.AddEdge(v, w, angularWeight);
                    }
                }
            }
        }



        /// <summary>
        /// Helper method to find the closest point by using RTree.
        /// needles can hold multiple test points.
        /// result contains the one of the most cloest point for each test point respectively.
        /// </summary>
        /// <param name="allNodes"></param>
        /// <param name="needles"></param>
        /// <returns></returns>
        private int[] FindCloestPoints(IList<Point3d> allNodes, IList<Point3d> needles)
        {
            var indicesArray = RTree.Point3dClosestPoints(allNodes, needles, double.MaxValue).ToArray();

            int[] result = new int[indicesArray.Length];
            for (int i = 0; i < indicesArray.Length; i++)
            {
                result[i] = indicesArray[i].First();
            }

            return result;
        }


        private double GetAngleOfTwoVectors(Vector3d v1, Vector3d v2)
        {
            double cosine = v1 * v2 / (v1.Length * v2.Length);

            // Convert radiant to degree. Math.Acos return 0~pi 
            return Math.Acos(cosine) / Math.PI * 180;
        }

        // Comparer is a good way to handle tolerance errors. 
        // With a certain tolerance, points are considered as equal.
        private class PointEqualityComparer : EqualityComparer<Point3d>
        {
            private readonly double _epsilon;

            private readonly int _round;

            public PointEqualityComparer(double epsilon)
            {
                _epsilon = epsilon;
                _round = (int)Math.Log10(1 / _epsilon);
            }

            public override bool Equals(Point3d x, Point3d y)
            {
                return x.Equals(y) || x.EpsilonEquals(y, _epsilon);
            }

            public override int GetHashCode(Point3d obj)
            {
                var pt = RoundPoint(obj);
                return pt.GetHashCode();
            }

            private Point3d RoundPoint(Point3d pt)
            {
                return new Point3d(Math.Round(pt.X, _round), Math.Round(pt.Y, _round), Math.Round(pt.Z, _round));
            }
        }
    }
}
