using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UrbanX.DataStructures.Graphs;
using UrbanX.DataStructures.Heaps;
using UrbanX.DataStructures.Trees;

namespace UrbanX.Algorithms.Graphs
{
    /// <summary>
    /// Calculating the centrality in a graph.
    /// This class has two properties: Betweenness, Closeness.
    /// </summary>
    /// <typeparam name="TGraph"></typeparam>
    /// <typeparam name="TVertex"></typeparam>
    public class CalculateCentrality<TGraph, TVertex> where TGraph : IGraph<TVertex>, IWeightedGraph<TVertex> where TVertex : IComparable<TVertex>
    {



        private readonly TGraph _graph;
        private readonly TVertex[] _vertices;
        private readonly Dictionary<TVertex, int> _verticesToIndices;

        /// <summary>
        /// The total betweenness centrality for every vertex in graph.
        /// </summary>
        public Dictionary<TVertex, double> Betweenness { get; }


        /// <summary>
        /// The total closeness centrality for every vertex in graph.
        /// Using concurrentDicionary for parallel computing.
        /// </summary>
        public ConcurrentDictionary<TVertex, double> Closeness { get; }


        public ConcurrentDictionary<TVertex, double> TotalDepth { get; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graph"></param>
        public CalculateCentrality(TGraph graph, bool normalise = false)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (graph.Edges.Any(edge => edge.Weight <= 0))
            {
                throw new ArgumentException("Negative and zero edge weight detected.");
            }

            _graph = graph;
            _vertices = graph.Vertices.ToArray();
            _verticesToIndices = new Dictionary<TVertex, int>(graph.VerticesCount);

            
            Betweenness = new Dictionary<TVertex, double>(graph.VerticesCount);

            Initialize();

            // Create closenss dictionary from betweeness to copy all the key(vertex)Value(0.0) pairs.
            Closeness = new ConcurrentDictionary<TVertex, double>(Betweenness);
            TotalDepth = new ConcurrentDictionary<TVertex, double>(Betweenness);

            Computing();

            if (normalise)
            {
                NormalizeBetweenness();
                NormalizeCloseness();
            }
        }


        private void Initialize()
        {
            for (int i = 0; i < _graph.VerticesCount; i++)
            {
                _verticesToIndices.Add(_vertices[i], i);
                Betweenness.Add(_vertices[i], 0.0);
            }
        }


        private void Computing()
        {
            // Run in parallel.
            if (_graph.VerticesCount >= 30)
            {
                int taskNumber = 30;
                var range = _graph.VerticesCount / taskNumber;
                List<Task> tasks = new List<Task>(taskNumber);

                // Local concurrent collection for parallel computing.
                ConcurrentBag<Dictionary<TVertex, double>> betweenessBag = new ConcurrentBag<Dictionary<TVertex, double>>();

                // Partitioning the vertices collection.
                IEnumerable<TVertex>[] verticesPartition = new IEnumerable<TVertex>[taskNumber];
                for (int i = 0; i < taskNumber; i++)
                {
                    var rangeCount = 0;
                    if (i == taskNumber - 1)
                    {
                        rangeCount = _graph.VerticesCount - i * range;
                    }
                    else
                    {
                        rangeCount = range;
                    }

                    verticesPartition[i] = _vertices.ToList().GetRange(i * range, rangeCount);
                }


                // Using for loop will encounter some errors due to int i will change during the process of each task. 
                // eg. for task1, i should be 1, but waitall task to complete, i has already changed.
                foreach (var tempVertices in verticesPartition)
                {
                    var t = Task.Run(() =>
                    {
                        var betweennessEachTask = new Dictionary<TVertex, double>(Betweenness);
                        foreach (var source in tempVertices)
                        {
                            var centrality = new CentralitySingleSource<TGraph, TVertex>(_graph, source, _verticesToIndices);
                            var tempScore = centrality.BetweennessScore;

                            foreach (var item in tempScore)
                            {
                                betweennessEachTask[item.Key] += item.Value;
                            }

                            Closeness.TryUpdate(source, centrality.CloesenessScore, 0.0);
                            TotalDepth.TryUpdate(source, centrality.TotalDepthScore, 0.0);
                        }

                        betweenessBag.Add(betweennessEachTask);
                    });

                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());

                foreach (var between in betweenessBag)
                {
                    foreach (var item in between)
                    {
                        Betweenness[item.Key] += item.Value;
                    }
                }
            }
            else
            {
                foreach (var source in _vertices)
                {
                    var centrality = new CentralitySingleSource<TGraph, TVertex>(_graph, source, _verticesToIndices);
                    var tempScore = centrality.BetweennessScore;

                    foreach (var item in tempScore)
                    {
                        Betweenness[item.Key] += item.Value;
                    }

                    Closeness[source] = centrality.CloesenessScore;
                    TotalDepth[source] = centrality.TotalDepthScore;
                }
            }
        }

        /// <summary>
        /// Normalizatoing the betweenness score by using (n - 1) * (n - 2).
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="normalize"></param>
        private void NormalizeBetweenness()
        {
            double scale;

            // if use int, will occur overflow error.
            double n = _graph.VerticesCount;

            if (n <= 2)
                return;


            if (_graph.IsDirected)
            {
                scale = 2.0 / ((n - 1) * (n - 2));
            }
            else
            {
                scale = 1.0 / ((n - 1) * (n - 2));
            }


            foreach (var vertex in _graph.Vertices)
            {
                Betweenness[vertex] *= scale;
            }

        }


        private void NormalizeCloseness()
        {
            

            int scale = _graph.VerticesCount - 1;

            if (scale <= 1)
                return;

            foreach (var vertex in _graph.Vertices)
            {
                Closeness[vertex] *= scale;
            }
        }
    }




    /// <summary>
    /// Internal class for computing the betweenness centrality for a single source.
    /// BetweennessScore is the dictionary with vertex as key and score as value.
    /// Every betweenness score has been normalized.
    /// </summary>
    /// <typeparam name="TGraph"></typeparam>
    /// <typeparam name="TVertex"></typeparam>
    internal class CentralitySingleSource<TGraph, TVertex> where TGraph : IGraph<TVertex>, IWeightedGraph<TVertex> where TVertex : IComparable<TVertex>
    {
        // Two consts as place holder for initializs arrays.
        private const double _infinity = double.PositiveInfinity;
        private readonly Dictionary<int, LinkedList<int>> _predecessors;
        private readonly double[] _distance;

        private readonly TVertex[] _vertices;
        private readonly MinPriorityQueue<int, double> _minPriorityQueue;

        // Fields for betweenness calculation
        private readonly Stack<int> stack;
        private readonly int[] sigma;
        private readonly double[] delta;

        /// <summary>
        /// Readonly dict means this class can't be reassigned, but all the dict method belond to this class can use.
        /// This dic is used for 1(O) TVertex query.
        /// </summary>
        private readonly Dictionary<TVertex, int> _nodesToIndices;

        private readonly TGraph _graph;
        private readonly TVertex _source;

        /// <summary>
        /// The partial result of betweenness centrality.
        /// </summary>
        public Dictionary<TVertex, double> BetweennessScore { get; }

        /// <summary>
        /// The closenes centrality for current source vertex. For Graph theory, cloesness should be 1 / sumDistance. For space syntax, using total depth to represent closeness which is  sumDistance.
        /// </summary>
        public double CloesenessScore { get; }


        /// <summary>
        /// Total depth equals to the sum of all the distances.
        /// </summary>
        public double TotalDepthScore { get; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        public CentralitySingleSource(TGraph graph, TVertex source, Dictionary<TVertex, int> verticesToIndices)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            _graph = graph;
            _source = source;
            _vertices = _graph.Vertices.ToArray();
            _nodesToIndices = verticesToIndices;

            // Instantiate all the containers withe vertices count as the initial capacity. 
            // For some fields, minHeap and stack, the maxisum capacity is the vertices count.
            // When part of the subgraphs are disconnected to graph, the vertices count of shortest path tree will be less than the graph.verticescount.
            var verticesCount = _graph.VerticesCount;

            _predecessors = new Dictionary<int, LinkedList<int>>(verticesCount);
            _minPriorityQueue = new MinPriorityQueue<int, double>(verticesCount);
            _distance = new double[verticesCount];

            BetweennessScore = new Dictionary<TVertex, double>(verticesCount);

            // stack.Count may less than vertices count.
            stack = new Stack<int>(verticesCount);
            // sigma and delta are for all the vertices, therefore they must have same length.
            sigma = new int[verticesCount];
            delta = new double[verticesCount];

            Initialize();
            Dijkstra();
            Accumulation();
            TotalDepthScore = GetTotalDepth();
            CloesenessScore = GetCloseness();
        }



        private void Initialize()
        {
            for (int i = 0; i < _graph.VerticesCount; i++)
            {
                _distance[i] = _infinity;
                _predecessors.Add(i, new LinkedList<int>());
                BetweennessScore.Add(_vertices[i], 0.0);
            }

            var sourceIndx = _nodesToIndices[_source];

            _distance[sourceIndx] = 0;
            _minPriorityQueue.Enqueue(sourceIndx, 0);
            _predecessors[sourceIndx].AddLast(sourceIndx);

            sigma[sourceIndx] = 1;
        }



        /// <summary>
        /// The Dijkstra's algorithm for one single source to all the destinations.
        /// CurrentVertex is v in graph theory, while adjacentVertex is w .
        /// </summary>
        private void Dijkstra()
        {
            while (!_minPriorityQueue.IsEmpty)
            {
                var currentVertexIndex = _minPriorityQueue.DequeueMin();

                var predecessors = _predecessors[currentVertexIndex];
                foreach (var pre in predecessors)
                {
                    if (pre == currentVertexIndex)
                    {
                        continue;
                    }

                    sigma[currentVertexIndex] += sigma[pre];
                }

                stack.Push(currentVertexIndex);

                var currentVertex = _vertices[currentVertexIndex];
                var outgoingEdges = _graph.OutgoingEdges(currentVertex);

                foreach (var outgoingEdge in outgoingEdges)
                {
                    var adjacentIndex = _nodesToIndices[outgoingEdge.Destination];


                    // The conditional operator ?:, also known as the ternary conditional operator.
                    var dist = _distance[currentVertexIndex] != _infinity ? _distance[currentVertexIndex] + outgoingEdge.Weight : _infinity;

                    if (dist < _distance[adjacentIndex])
                    {
                        // update distTo and edgeTo
                        _distance[adjacentIndex] = dist;
                        // update sigma, becasue of finding a new shortest path to adjacent node.
                        sigma[adjacentIndex] = 0;

                        // Find the shorter path, therefore we need to update the predecessors by cleaning the linkedlist.
                        _predecessors[adjacentIndex].Clear();
                        _predecessors[adjacentIndex].AddLast(currentVertexIndex);                   

                        if (_minPriorityQueue.Contains(adjacentIndex))
                        {
                            _minPriorityQueue.UpdatePriority(adjacentIndex, dist);
                        }
                        else
                        {
                            _minPriorityQueue.Enqueue(adjacentIndex, dist);                           
                        }
                        
                    }
                    // Handle equal distance. Meaning there are multiply shortest paths to vertex w.
                    else if (dist == _distance[adjacentIndex])
                    {
                        sigma[adjacentIndex] += sigma[currentVertexIndex];
                        _predecessors[adjacentIndex].AddLast(currentVertexIndex);
                    }
                }
            }
        }

        private void Accumulation()
        {
            while (stack.Count != 0)
            {
                // w vertex
                var currentVertexIndex = stack.Pop();
                var coeff = (1.0 + delta[currentVertexIndex]) / sigma[currentVertexIndex];

                // Find the predecessors v of current vertex w.
                var predecessors = _predecessors[currentVertexIndex];
                foreach (var v in predecessors)
                {
                    delta[v] += sigma[v] * coeff;
                }

                if (currentVertexIndex != _nodesToIndices[_source])
                {
                    // Include end node
                    BetweennessScore[_vertices[currentVertexIndex]] += delta[currentVertexIndex];
                }
            }
        }


        private double GetTotalDepth()
        {
            double d = 0;
            for (int i = 0; i < _distance.Length; i++)
            {
                if (_distance[i] != _infinity)
                    d += _distance[i];
            }

            return d;
        }


        private double GetCloseness()
        {
            if (TotalDepthScore == 0)
                return 0;
            else
                return 1/ TotalDepthScore;
        }

        


        //private void Accumulation()
        //{
        //    BetweennessScore[_source] += stack.Count - 1;

        //    while (stack.Count != 0)
        //    {
        //        // w vertex
        //        var currentVertexIndex = stack.Pop();
        //        var coeff = (1.0 + delta[currentVertexIndex]) / sigma[currentVertexIndex];

        //        // Find the predecessors v of current vertex w.
        //        var predecessors = _predecessors[currentVertexIndex];
        //        foreach (var v in predecessors)
        //        {
        //            delta[v] += sigma[v] * coeff;
        //        }

        //        if (currentVertexIndex != _nodesToIndices[_source])
        //        {
        //            // Include end node
        //            BetweennessScore[_vertices[currentVertexIndex]] += delta[currentVertexIndex]+1;
        //        }
        //    }
        //}



        /// <summary>
        /// Obsoleted. 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private static double SumTree(ITreeNode<double> root)
        {
            if (root == null)
                return 0;

            return root.Value + SumTree(root.LeftChild) + SumTree(root.RightChild);
        }

    }
}
