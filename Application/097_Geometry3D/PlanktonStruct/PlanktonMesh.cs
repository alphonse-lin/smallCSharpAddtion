using System;
using System.Collections.Generic;
using System.Linq;

namespace UrbanX.Application.Geometry
{
    /// <summary>
    /// This is the main class that describes a plankton mesh.
    /// </summary>
    public class PlanktonMesh
    {
        private PlanktonVertexList _vertices;
        private PlanktonHalfedgeList _halfedges;
        private PlanktonFaceList _faces;

        #region "constructors"
        /// <summary>
        /// Initializes a new (empty) instance of the <see cref="PlanktonMesh"/> class.
        /// </summary>
        public PlanktonMesh() { }

        /// <summary>
        /// Initializes a new (duplicate) instance of the <see cref="PlanktonMesh"/> class.
        /// </summary>
        public PlanktonMesh(PlanktonMesh source)
        {
            foreach (var v in source.Vertices)
            {
                this.Vertices.Add(new PlanktonVertex()
                {
                    OutgoingHalfedge=v.OutgoingHalfedge,
                    X=v.X,
                    Y=v.Y,
                    Z=v.Z
                });
            }
            foreach (var f in source.Faces)
            {
                this.Faces.Add(new PlanktonFace() { FirstHalfedge=f.FirstHalfedge});
            }
            foreach (var h in source.Halfedges)
            {
                this.Halfedges.Add(new PlanktonHalfedge()
                {
                    StartVertex = h.StartVertex,
                    AdjacentFace = h.AdjacentFace,
                    NextHalfedge = h.NextHalfedge,
                    PrevHalfedge = h.PrevHalfedge,
                });
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets access to the <see cref="PlanktonVertexList"/> collection in this mesh.
        /// </summary>
        public PlanktonVertexList Vertices
        {
            get { return _vertices ?? (_vertices = new PlanktonVertexList(this)); }
        }

        /// <summary>
        /// Gets access to the <see cref="PlanktonHalfedgeList"/> collection in this mesh.
        /// </summary>
        public PlanktonHalfedgeList Halfedges
        {
            get { return _halfedges ?? (_halfedges = new PlanktonHalfedgeList(this)); }
        }

        /// <summary>
        /// Gets access to the <see cref="PlanktonFaceList"/> collection in this mesh.
        /// </summary>
        public PlanktonFaceList Faces
        {
            get { return _faces ?? (_faces = new PlanktonFaceList(this)); }
        }
        #endregion

    }
}
