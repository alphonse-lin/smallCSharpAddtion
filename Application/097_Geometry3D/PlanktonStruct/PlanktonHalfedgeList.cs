using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UrbanX.Application.Geometry
{
    /// <summary>
    /// Provides access to the halfedges and <see cref="PlanktonHalfedge"/> related functionality of a Mesh.
    /// </summary>
    public class PlanktonHalfedgeList : IEnumerable<PlanktonHalfedge>
    {
        private readonly PlanktonMesh _mesh;
        private List<PlanktonHalfedge> _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanktonHalfedgeList"/> class.
        /// Should be called from the mesh constructor.
        /// </summary>
        /// <param name="owner">The <see cref="PlanktonMesh"/> to which this list of halfedges belongs.</param>
        internal PlanktonHalfedgeList(PlanktonMesh owner)
        {
            this._list = new List<PlanktonHalfedge>();
            this._mesh = owner;
        }

        /// <summary>
        /// Gets the number of halfedges.
        /// </summary>
        public int Count
        {
            get
            {
                return this._list.Count;
            }
        }

        #region methods
        #region halfedge access
        /// <summary>
        /// Adds a new halfedge to the end of the Halfedge list.
        /// </summary>
        /// <param name="halfEdge">Halfedge to add.</param>
        /// <returns>The index of the newly added halfedge.</returns>
        public int Add(PlanktonHalfedge halfedge)
        {
            if (halfedge == null) return -1;
            this._list.Add(halfedge);
            return this.Count - 1;
        }

        /// <summary>
        /// Add a pair of halfedges to the mesh.
        /// </summary>
        /// <param name="start">A vertex index (from which the first halfedge originates).</param>
        /// <param name="end">A vertex index (from which the second halfedge originates).</param>
        /// <param name="face">A face index (adjacent to the first halfedge).</param>
        /// <returns>The index of the first halfedge in the pair.</returns>
        internal int AddPair(int start, int end, int face)
        {
            int i = this.Count;
            this.Add(new PlanktonHalfedge(start, face, i + 1));
            this.Add(new PlanktonHalfedge(end, -1, i));
            return i;
        }

        internal void RemovePariHelper(int index)
        {
            int pair = this.GetPairHalfedge(index);

            //Reconnect adjacent halfedges
            this.MakeConsecutive(this[pair].PrevHalfedge, this[index].NextHalfedge);
            this.MakeConsecutive(this[index].PrevHalfedge, this[pair].NextHalfedge);

            // Update vertices' outgoing halfedges, if necessary. If last halfedge then
            // make vertex unused (outgoing == -1), otherwise set to next around vertex.
            var v1 = _mesh.Vertices[this[index].StartVertex];
            var v2 = _mesh.Vertices[this[pair].StartVertex];
            if (v1.OutgoingHalfedge == index)
            {
                if (this[pair].NextHalfedge == index) { v1.OutgoingHalfedge = -1; }
                else { v1.OutgoingHalfedge = this[pair].NextHalfedge; }
            }
            if (v2.OutgoingHalfedge == pair)
            {
                if (this[index].NextHalfedge == pair) { v2.OutgoingHalfedge = -1; }
                else { v2.OutgoingHalfedge = this[index].NextHalfedge; }
            }

            // Mark halfedges for deletion
            this[index] = PlanktonHalfedge.Unset;
            this[pair] = PlanktonHalfedge.Unset;
        }


        /// <summary>
        /// Returns the <see cref="PlanktonHalfedge"/> at the given index.
        /// </summary>
        /// <param name="index">
        /// Index of halfedge to get.
        /// Must be larger than or equal to zero and smaller than the Halfedge Count of the mesh.
        /// </param>
        /// <returns>The halfedge at the given index.</returns>

        public PlanktonHalfedge this[int index]
        {
            get { return this._list[index]; }
            internal set { this._list[index] = value; }
        }

        #region internal helpers
        internal void MakeConsecutive(int prev,int next)
        {
            this[prev].NextHalfedge = next;
            this[next].PrevHalfedge = prev;
        }
        #endregion

        /// <summary>
        /// Gets the opposing halfedge in a pair.
        /// </summary>
        /// <param name="index">A halfedge index.</param>
        /// <returns>The halfedge index with which the specified halfedge is paired.</returns>
        public int GetPairHalfedge(int halfedgeIndex)
        {
            if (halfedgeIndex<0||halfedgeIndex>=this.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            return halfedgeIndex % 2 == 0 ? halfedgeIndex + 1 : halfedgeIndex - 1;
        }

        #endregion

        internal

        #endregion

        public IEnumerator<PlanktonHalfedgeList> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator<PlanktonHalfedge> IEnumerable<PlanktonHalfedge>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
