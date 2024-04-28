using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;
using GeometRi;
using Tekla.Structures.Solid;
using TSG = Tekla.Structures.Geometry3d;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;
using Tekla.Structures;
using System.Numerics;
using Tekla.Structures.ModelInternal;

namespace ConnectionClassifier.GeometryCalculations
{
    public class BoundingBox
    {
        public Coord3d CreateCoord3DFromTeklaCS(Part obj)
        {
            var teklaCS = obj.GetCoordinateSystem();
            var originTekla = teklaCS.Origin;
            var xTekla = teklaCS.AxisX;
            var yTekla = teklaCS.AxisY;

            var origin = new GeometRi.Point3d(originTekla.X, originTekla.Y, originTekla.Z);
            var x = new GeometRi.Vector3d(xTekla.X, xTekla.Y, xTekla.Z);
            var y = new GeometRi.Vector3d(yTekla.X, yTekla.Y, yTekla.Z);

            var plane = new Coord3d(origin, x, y);

            return plane;
        }

        public HashSet<Point3d> GetSolidPoints(Part obj)
        {
            var solid = obj.GetSolid();
            EdgeEnumerator edgeEnumerator = solid.GetEdgeEnumerator();
            HashSet<Point3d> pointList = new HashSet<Point3d>();

            while (edgeEnumerator.MoveNext())
            {
                var edge = edgeEnumerator.Current as Edge;

                Point start = edge.StartPoint;
                Point end = edge.EndPoint;

                Point3d startPoint = new Point3d(start.X, start.Y,start.Z);
                Point3d endPoint = new Point3d(end.X, end.Y, end.Z);

                pointList.Add(startPoint);
                pointList.Add(endPoint);
            }

            return pointList;
        }

        public class MinMaxCoordinates
        {
            public double MinX { get; set; }
            public double MinY { get; set; }
            public double MinZ { get; set; }
            public double MaxX { get; set; }
            public double MaxY { get; set; }
            public double MaxZ { get; set; }
        }

        public DomainsClass GetMinMaxCoordinates(HashSet<Point3d> points)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;
            var minZ = double.MaxValue;
            var maxZ = double.MinValue;

            foreach (Point3d point in points)
            {
                if (point.X < minX)
                {
                    minX = point.X;
                }
                if (point.X > maxX)
                {
                    maxX = point.X;
                }

                if (point.Y < minY)
                {
                    minY = point.Y;
                }
                if (point.Y > maxY)
                {
                    maxY = point.Y;
                }

                if (point.Z < minZ)
                {
                    minZ = point.Z;
                }
                if (point.Z > maxZ)
                {
                    maxZ = point.Z;
                }
            }
            var minMaxCoordinates = new DomainsClass()
            {
                StartX = minX,
                StartY = minY,
                StartZ = minZ,
                EndX = maxX,
                EndY = maxY,
                EndZ = maxZ
            };
            return minMaxCoordinates;
        }


        public Box3d CreateBboxInPlane(HashSet<Point3d> pointList, Coord3d plane, Part obj)
        {
            HashSet<Point3d> pointsLocal = new HashSet<Point3d>();

            foreach(Point3d pointGlobal in pointList)
            {
                Point3d pointLocal = pointGlobal.ConvertTo(plane);
                pointsLocal.Add(pointLocal);
            }

            DomainsClass localMinMax = GetMinMaxCoordinates(pointsLocal);

            var minX = localMinMax.StartX;
            var maxX = localMinMax.EndX;
            var minY = localMinMax.StartY;
            var maxY = localMinMax.EndY;
            var minZ = localMinMax.StartZ;
            var maxZ = localMinMax.EndZ;                        

            var lx = (maxX - minX);
            var ly = (maxY - minY);
            var lz = (maxZ - minZ);

            var centerX = minX + (maxX - minX) / 2;
            var centerY = minY + (maxY - minY) / 2;
            var centerZ = minZ + (maxZ - minZ) / 2;

            var centerPoint = new Point3d(centerX, centerY, centerZ, plane);

            var bbox = new Box3d(centerPoint, lx, ly, lz, plane);

            DomainsClass domains = new DomainsClass()
            {
                StartX = minX,
                StartY = minY,
                StartZ = minZ,
                EndX = maxX,
                EndY = maxY,
                EndZ = maxZ
            };
            return bbox;
        }

        public List<PartObject> GetClassificationPartObjects()
        {
            List<PartObject> partObjects = new List<PartObject>();

            Model model = new Model();

            Tekla.Structures.Model.UI.ModelObjectSelector Selector = new Tekla.Structures.Model.UI.ModelObjectSelector();
            ModelObjectEnumerator listObjects = Selector.GetSelectedObjects();

            int partCount = 0;

            foreach (Part obj in listObjects)
            {
                partCount++;
                var plane = CreateCoord3DFromTeklaCS(obj);
                HashSet<Point3d> pointList = GetSolidPoints(obj);

                DomainsClass globalMinMax = GetMinMaxCoordinates(pointList);

                var bbox = CreateBboxInPlane(pointList, plane, obj);

                var drawer = new DrawObjectsInTekla();
                //drawer.DrawBBox(bbox);

                string guid = obj.Identifier.GUID.ToString();

                string profile = obj.Profile.ProfileString;
                string material = obj.Material.MaterialString;
                string tekla_class = obj.Class;

                double width = double.NaN;
                obj.GetReportProperty("WIDTH", ref width);

                double height = double.NaN;
                obj.GetReportProperty("HEIGHT", ref height);

                double length = double.NaN;
                obj.GetReportProperty("LENGTH", ref length);

                PartObject partObject = new PartObject() { 
                    BBOX = bbox, 
                    Domains = globalMinMax,
                    CS = plane, 
                    GUID = guid, 
                    Profile = profile,
                    Material = material,
                    TeklaClass= tekla_class,
                    Height = height,
                    Width = width,
                    Length = length,
                    modelObjectPart = obj};

                partObjects.Add(partObject);
            }
            return partObjects;
        }
        //public bboxOverlap
    }
}
