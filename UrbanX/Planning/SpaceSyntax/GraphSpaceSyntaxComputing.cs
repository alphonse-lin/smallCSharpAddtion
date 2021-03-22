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
    public class GraphSpaceSyntaxComputing
    {

        /// <summary>
        /// The total betweenness centrality for every vertex in graph.
        /// </summary>
        public Dictionary<int, double> MetricBetweenness { get; }

        public Dictionary<int, double> AngularBetweenness { get; }
        /// <summary>
        /// The total closeness centrality for every vertex in graph.
        /// Using concurrentDicionary for parallel computing.
        /// </summary>
        public Dictionary<int, double> MetricCloseness { get; }

        public Dictionary<int, double> AngularCloseness { get; }


        public Dictionary<int, double> MetricIntergration { get; }

        public Dictionary<int, double> AngularIntergration { get; }

        /// <summary>
        /// NACH=log(value("T1024 Choice R1000 metric")+1)/log(value("T1024 Total Depth R1000 metric")+100)
        /// </summary>        
        public Dictionary<int, double> MetricNach { get; }


        public Dictionary<int, double> AngularNach { get; }

        public GraphSpaceSyntaxComputing(UndirectedWeightedSparseGraph<int> metricGraph, UndirectedWeightedSparseGraph<int> angularGraph, double radius=-1, bool normalise = false)
        {

            if(radius == -1)
            {
                var computeMetric = new CalculateCentrality<UndirectedWeightedSparseGraph<int>, int>(metricGraph, normalise);

                MetricBetweenness = computeMetric.Betweenness;
                MetricCloseness = new Dictionary<int, double>(computeMetric.Closeness);
                var metricTotalDepth = new Dictionary<int, double>(computeMetric.TotalDepth);
                MetricIntergration = ComputeIntergration(metricGraph, metricTotalDepth);
                MetricNach =  ComputeNach(metricGraph, MetricBetweenness, metricTotalDepth);

                var computeAngular = new CalculateCentrality<UndirectedWeightedSparseGraph<int>, int>(angularGraph, normalise);

                AngularBetweenness = computeAngular.Betweenness;
                AngularCloseness = new Dictionary<int, double>(computeAngular.Closeness);
                var angularTotalDepth = new Dictionary<int, double>(computeAngular.TotalDepth);
                AngularIntergration = ComputeIntergration(angularGraph, angularTotalDepth);
                AngularNach = ComputeNach(angularGraph, AngularBetweenness, angularTotalDepth);
            }
            else
            {
                // Has radius.
 
                // Step1: compute metric graph.
                var computeMetric = new CalculateCentralityRadius<UndirectedWeightedSparseGraph<int>, int>(metricGraph,radius, normalise);

                MetricBetweenness = computeMetric.Betweenness;
                MetricCloseness = new Dictionary<int, double>(computeMetric.Closeness);
                var metricTotalDepth = new Dictionary<int, double>(computeMetric.TotalDepth);
                MetricIntergration = ComputeIntergration(metricGraph, metricTotalDepth);
                MetricNach = ComputeNach(metricGraph, MetricBetweenness, metricTotalDepth);


                // Step2: get sub graph within radius.
                var subGraphs = computeMetric.SubGraphs;
                var computeAngular = new CalculateCentralitySubgraphs<UndirectedWeightedSparseGraph<int>, int>(angularGraph, subGraphs, normalise);

                AngularBetweenness = computeAngular.Betweenness;
                AngularCloseness = new Dictionary<int, double>(computeAngular.Closeness);
                var angularTotalDepth = new Dictionary<int, double>(computeAngular.TotalDepth);
                AngularIntergration = ComputeIntergration(angularGraph, angularTotalDepth);
                AngularNach = ComputeNach(angularGraph, AngularBetweenness, angularTotalDepth);
            }
        }



        private Dictionary<int, double> ComputeIntergration(UndirectedWeightedSparseGraph<int> graph, Dictionary<int, double> totalDepth )
        {
            var n = graph.VerticesCount;

            var intergration = new Dictionary<int, double>(graph.VerticesCount);
            foreach (var vertex in graph.Vertices)
            {
                intergration[vertex] = Math.Log((n - 2) / 2) / Math.Log(totalDepth[vertex] - n + 1);
            }

            return intergration;
        }


        private Dictionary<int, double> ComputeNach(UndirectedWeightedSparseGraph<int> graph, Dictionary<int, double> betweenness, Dictionary<int, double> totalDepth)
        {
            var nach = new Dictionary<int, double>(graph.VerticesCount);

            foreach (var vertex in graph.Vertices)
            {
                nach[vertex] = Math.Log(betweenness[vertex] + 1) / Math.Log(totalDepth[vertex] + 100);
            }

            return nach;
        }
    }
}
