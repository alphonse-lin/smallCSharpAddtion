using System;
using System.Collections.Generic;

using UrbanX.Algorithms.Graphs;
using UrbanX.DataStructures.Graphs;

namespace UrbanX.Planning.SpaceSyntax
{
    /// <summary>
    /// A wrapper for calculating cenrality class in algorithms namespace.
    /// This is used for spacesyntax only by using the undirected weighted graph.
    /// </summary>
    public class GraphCentrality
    {

        private readonly UndirectedWeightedSparseGraph<int> _graph;
        /// <summary>
        /// The total betweenness centrality for every vertex in graph.
        /// </summary>
        public Dictionary<int, double> Betweenness { get; }


        /// <summary>
        /// The total closeness centrality for every vertex in graph.
        /// Using concurrentDicionary for parallel computing.
        /// </summary>
        public Dictionary<int, double> Closeness { get; }

        public Dictionary<int, double> TotalDepth { get; } 

        public Dictionary<int, double> Intergration { get; }



        /// <summary>
        /// NACH=log(value("T1024 Choice R1000 metric")+1)/log(value("T1024 Total Depth R1000 metric")+100)
        /// </summary>        
        public Dictionary<int, double> Nach { get; }


        public GraphCentrality(UndirectedWeightedSparseGraph<int> graph, bool normalize)
        {
            _graph = graph;

            var computeResult = new CalculateCentrality<UndirectedWeightedSparseGraph<int>, int>(_graph, normalize);
            Betweenness = computeResult.Betweenness;
            Closeness = new Dictionary<int, double>(computeResult.Closeness);
            TotalDepth = new Dictionary<int, double>(computeResult.TotalDepth);
   
            Intergration = new Dictionary<int, double>(Betweenness.Count);
            Nach = new Dictionary<int, double>(Betweenness.Count);

            ComputeIntergration();
            ComputeNach();
        }


        private void ComputeIntergration()
        {
            double n = _graph.VerticesCount;

            foreach (var vertex in _graph.Vertices)
            {
                Intergration[vertex] = Math.Log((n - 2) / 2) / Math.Log(TotalDepth[vertex] - n + 1);
            }
        }

        private void ComputeNach()
        {
            foreach (var vertex in _graph.Vertices)
            {
                Nach[vertex] = Math.Log(Betweenness[vertex] + 1) / Math.Log(TotalDepth[vertex] + 100);
            }
        }

    }
}
