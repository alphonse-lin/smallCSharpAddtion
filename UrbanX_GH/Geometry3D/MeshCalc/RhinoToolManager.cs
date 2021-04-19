using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using Rh=Rhino.Geometry;
using System.Linq;

namespace UrbanX_GH.Application.Geometry
{
    public class RhinoToolManager
    {
        public static double GetMaxBounds(Rh.Brep brep)
        {
            var ptMax=brep.GetBoundingBox(false).Max;
            var ptMin = brep.GetBoundingBox(false).Min;
            return ptMax.DistanceTo(ptMin);
        }

        public static DMesh3 ConvertFromRhMesh(IEnumerable<Rh.Mesh> meshInput)
        {
            var tempMesh = new Rh.Mesh();
            for (int i = 0; i < meshInput.Count(); i++)
            {
                tempMesh.Append(meshInput.ElementAt(i));
            }

            var meshVertices = ConvertFromRhPt(tempMesh.Vertices.ToPoint3fArray());
            var meshNormals = ConvertFromRhPt(tempMesh.Normals.ToFloatArray());
            var meshFaces = ConvertFromRhinoMeshFace(tempMesh.Faces);
            DMesh3 meshOut = DMesh3Builder.Build(meshVertices, meshFaces, meshNormals);
            return meshOut;
        }

        private static Vector3f[] ConvertFromRhPt(Rh.Point3f[] ptArray)
        {
            var vectorResult = new Vector3f[ptArray.Length];
            for (int i = 0; i < ptArray.Length; i++)
            {
                vectorResult[i] = new Vector3f(ptArray[i].X, ptArray[i].Y, ptArray[i].Z);
            }
            return vectorResult;
        }

        private static Vector3f[] ConvertFromRhPt(float[] floatArray)
        {
            var vectorResult = new Vector3f[floatArray.Count()/3];
            for (int i = 0; i < vectorResult.Length; i++)
            {
                vectorResult[i].x = floatArray[i*3];
                vectorResult[i].y = floatArray[i*3+1];
                vectorResult[i].z = floatArray[i*3+2];
            }
            return vectorResult;
        }

        private static Index3i[] ConvertFromRhinoMeshFace(Rh.Collections.MeshFaceList meshFaceList)
        {
            Index3i[] triangles = new Index3i[meshFaceList.Count];
            for (int i = 0; i < meshFaceList.Count; i++)
            {
                var meshFace = meshFaceList[i];
                triangles[i] = new Index3i(meshFace.A, meshFace.B, meshFace.C) ;
            }
            return triangles;
        }



    }
}
