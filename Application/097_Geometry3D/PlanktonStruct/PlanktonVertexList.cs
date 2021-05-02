using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UrbanX.Application.Geometry
{
    public class PlanktonVertexList : IEnumerable<PlanktonVertex>
    {
        private readonly PlanktonMesh _mesh;
        private List<PlanktonVertex> _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanktonVertexList"/> class.
        /// Should be called from the mesh constructor.
        /// </summary>
        /// <param name="owner">The <see cref="PlanktonMesh"/> to which this list of vertices belongs.</param>
        internal PlanktonVertexList(PlanktonMesh owner)
        {
            this._list = new List<PlanktonVertex>();
            this._mesh = owner;
        }

        /// <summary>
        /// Gets the number of vertices.
        /// </summary>
        public int Count
        {
            get
            {
                return this._list.Count;
            }
        }
        
        #region vertex access
        #region Adding
        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="vertex">Vertex to add.</param>
        /// <returns>The index of the newly added vertex.</returns>
        internal int Add(PlanktonVertex vertex)
        {
            if (vertex == null) return -1;
            this._list.Add(vertex);
            return this.Count - 1;
        }

        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="vertex">Vertex to add.</param>
        /// <returns>The index of the newly added vertex.</returns>
        internal int Add(PlanktonXYZ vertex)
        {
            this._list.Add(new PlanktonVertex(vertex.X, vertex.Y, vertex.Z));
            return this.Count - 1;
        }

        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="x">X component of new vertex coordinate.</param>
        /// <param name="y">Y component of new vertex coordinate.</param>
        /// <param name="z">Z component of new vertex coordinate.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public int Add(double x, double y, double z)
        {
            return this.Add(new PlanktonVertex(x, y, z));
        }

        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="x">X component of new vertex coordinate.</param>
        /// <param name="y">Y component of new vertex coordinate.</param>
        /// <param name="z">Z component of new vertex coordinate.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public int Add(float x, float y, float z)
        {
            return this.Add(new PlanktonVertex(x, y, z));
        }
        #endregion

        /// <summary>
        /// Adds a series of new vertices to the end of the vertex list.
        /// </summary>
        /// <param name="vertices">A list, an array or any enumerable set of <see cref="PlanktonXYZ"/>.</param>
        /// <returns>Indices of the newly created vertices.</returns>
        public int[] AddVertices(IEnumerable<PlanktonXYZ> vertices)
        {
            return vertices.Select(v=>this.Add(v)).ToArray();
        }

        /// <summary>
        /// Returns the <see cref="PlanktonVertex"/> at the given index.
        /// </summary>
        /// <param name="index">
        /// Index of vertex to get.
        /// Must be larger than or equal to zero and smaller than the Vertex Count of the mesh.
        /// </param>
        /// <returns>The vertex at the given index.</returns>
        public PlanktonVertex this[int index]
        {
            get { return this._list[index]; }
            internal set { this._list[index] = value; }
        }

        /// <summary>
        /// <para>Sets or adds a vertex to the Vertex List.</para>
        /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
        /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para>
        /// <para>If [index] is larger than [Count], the function will return false.</para>
        /// </summary>
        /// <param name="vertexIndex">Index of vertex to set.</param>
        /// <param name="x">X component of vertex location.</param>
        /// <param name="y">Y component of vertex location.</param>
        /// <param name="z">Z component of vertex location.</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public bool SetVertex(int vertexIndex, float x, float y, float z)
        {
            if (vertexIndex>=0 && vertexIndex<_list.Count)
            {
                var v = this._list[vertexIndex];
                v.X = x;
                v.Y = y;
                v.Z = z;
            }
            else if (vertexIndex == _list.Count)
            {
                this.Add(x, y, z);
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// <para>Sets or adds a vertex to the Vertex List.</para>
        /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
        /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para>
        /// <para>If [index] is larger than [Count], the function will return false.</para>
        /// </summary>
        /// <param name="vertexIndex">Index of vertex to set.</param>
        /// <param name="x">X component of vertex location.</param>
        /// <param name="y">Y component of vertex location.</param>
        /// <param name="z">Z component of vertex location.</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public bool SetVertex(int vertexIndex, double x, double y, double z)
        {
            if (vertexIndex>=0 && vertexIndex <_list.Count)
            {
                var v = this._list[vertexIndex];
                v.X = (float)x;
                v.Y = (float)y;
                v.Z = (float)z;
            }
            else if (vertexIndex==_list.Count)
            {
                this.Add(x, y, z);
            }
            else { return false; }
            return true;
        }
#endregion
        public IEnumerator<PlanktonVertex> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
