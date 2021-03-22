using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SketchUpNET;

namespace UrbanX.Application.Geometry
{
    public class SketchUpManager
    {
        public static SketchUp GetMeshFromSkp(string filePath)
        {
            SketchUp skp = new SketchUp();
            skp.LoadModel(filePath);
            return skp;
        }
    }
}