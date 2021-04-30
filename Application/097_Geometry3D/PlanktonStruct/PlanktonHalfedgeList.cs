using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UrbanX.Application.Geometry
{
    public class PlanktonHalfedgeList : IEnumerable<PlanktonHalfedgeList>
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

        }

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

        #endregion

        public IEnumerator<PlanktonHalfedgeList> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
