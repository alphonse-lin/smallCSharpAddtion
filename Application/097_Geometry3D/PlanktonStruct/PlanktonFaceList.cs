using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UrbanX.Application.Geometry
{
    /// <summary>
    /// Provides access to the faces and <see cref="PlanktonFace"/> related functionality of a Mesh.
    /// </summary>
    public class PlanktonFaceList : IEnumerable<PlanktonFace>
    {
        private readonly PlanktonMesh _mesh;
        private List<PlanktonFace> _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanktonFaceList"/> class.
        /// Should be called from the mesh constructor.
        /// </summary>
        /// <param name="owner">The <see cref="PlanktonMesh"/> to which this list of half-edges belongs.</param>
        internal PlanktonFaceList(PlanktonMesh owner)
        {
            this._list = new List<PlanktonFace>();
            this._mesh = owner;
        }

        /// <summary>
        /// Gets the number of faces.
        /// </summary>
        public int Count
        {
            get
            {
                return this._list.Count;
            }
        }

        #region methods
        #region face access
        /// <summary>
        /// Adds a new face to the end of the Face list.
        /// </summary>
        /// <param name="halfEdge">Face to add.</param>
        /// <returns>The index of the newly added face.</returns>
        internal int Add(PlanktonFace face)
        {
            if (face == null) return -1;
            this._list.Add(face);
            return this.Count - 1;
        }

        public int AddFace(IEnumerable<int> indices)
        {
            int[] array = indices.ToArray();

            var hs=_mesh
        }




        #endregion
        #endregion


        public IEnumerator<PlanktonFace> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
