using System;
using Rhino.Geometry;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            Mesh debugMesh = new Mesh();
            debugMesh.TopologyVertices.ConnectedTopologyVertices(0);
            Console.WriteLine("Hello World!");
        }
    }
}
