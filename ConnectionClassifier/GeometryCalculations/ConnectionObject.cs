using GeometRi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionClassifier.GeometryCalculations
{
    public class ConnectionObject
    {
        public PartObject Part1 { get; set; }
        public PartObject Part2 { get; set;}
        public PlaneParameters Plane1Parameters { get; set; }
        public PlaneParameters Plane2Parameters { get; set; }
        public ConnectionAnglesClass ConnectionAngles { get; set; }
        public string ConnetionType { get; set; }
    }


    public class PlaneParameters
    {
        public double xStart { get; set; }
        public double xOverlap { get; set; }
        public double xEnd { get; set; }
        public double yStart { get; set; }
        public double yOverlap { get; set; }
        public double yEnd { get; set; }
        public double zStart { get; set; }
        public double zOverlap { get; set; }
        public double zEnd { get; set; }
        public Point3d ConnectionStartPoint { get; set; }
        public Point3d ConnectionEndPoint { get; set;  }
        
    }

    public class ConnectionAnglesClass
    {
        public double AngleXX { get; set; }
        public double AngleXY { get; set; }
        public double AngleXZ { get; set; }
        public double AngleYX { get; set; }
        public double AngleYY { get; set; }
        public double AngleYZ { get; set; }
        public double AngleZX { get; set; }
        public double AngleZY { get; set; }
        public double AngleZZ { get; set; }
    }


}
