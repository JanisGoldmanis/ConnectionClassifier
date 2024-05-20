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
using System.Windows;

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

                Tekla.Structures.Geometry3d.Point start = edge.StartPoint;
                Tekla.Structures.Geometry3d.Point end = edge.EndPoint;

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

        public double CalculateBboxRadius(Box3d box)
        {
            double height = box.L1 / 2;
            double width = box.L2 / 2;
            double length = box.L3 / 2;
            double diagonal = Math.Sqrt(Math.Pow(height, 2) + Math.Pow(width, 2) + Math.Pow(length, 2));
            return diagonal;
        }


        public static void UpdateModelParameters(Box3d box, PartObject partObject, ModelParameters modelParameters)
        {
            Point3d centerPoint = box.Center.ConvertToGlobal();

            if(centerPoint.X > modelParameters.maxX)
            {
                modelParameters.maxX = centerPoint.X;
            }
            if(centerPoint.X < modelParameters.minX)
            {
                modelParameters.minX = centerPoint.X;
            }
            if (centerPoint.Y > modelParameters.maxY)
            {
                modelParameters.maxY = centerPoint.Y;
            }
            if (centerPoint.Y < modelParameters.minY)
            {
                modelParameters.minY = centerPoint.Y;
            }
            if (centerPoint.Z > modelParameters.maxZ)
            {
                modelParameters.maxZ = centerPoint.Z;
            }
            if (centerPoint.Z < modelParameters.minZ)
            {
                modelParameters.minZ = centerPoint.Z;
            }

            double lengthX = partObject.Domains.EndX - partObject.Domains.StartX;
            double lengthY = partObject.Domains.EndY - partObject.Domains.StartY;
            double lengthZ = partObject.Domains.EndZ - partObject.Domains.StartZ;

            if (lengthX > modelParameters.maxXLength)
            {
                modelParameters.maxXLength = lengthX;
            }
            if (lengthY > modelParameters.maxYLength)
            {
                modelParameters.maxYLength = lengthY;
            }
            if (lengthZ > modelParameters.maxZLength)
            {
                modelParameters.maxZLength = lengthZ;
            }
        }

        public (List<PartObject>, ModelParameters) GetClassificationPartObjects()
        {
            ModelParameters modelParameters = new ModelParameters()
            {
                minX = double.MaxValue,
                minY = double.MaxValue,
                minZ = double.MaxValue,
                maxX = double.MinValue,
                maxY = double.MinValue,
                maxZ = double.MinValue,
                maxXLength = 0,
                maxYLength = 0,
                maxZLength = 0,
            };

            List<PartObject> partObjects = new List<PartObject>();

            Model model = new Model();

            Tekla.Structures.Model.UI.ModelObjectSelector Selector = new Tekla.Structures.Model.UI.ModelObjectSelector();
            ModelObjectEnumerator listObjects = Selector.GetSelectedObjects();

            int partCount = 0;

            foreach (Part obj in listObjects)
            {
                try
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

                    double radius = CalculateBboxRadius(bbox);

                    double cogZ = bbox.Center.ConvertToGlobal().Z;

                    PartObject partObject = new PartObject()
                    {
                        BBOX = bbox,
                        Domains = globalMinMax,
                        CS = plane,
                        GUID = guid,
                        Profile = profile,
                        Material = material,
                        TeklaClass = tekla_class,
                        Height = height,
                        Width = width,
                        Length = length,
                        modelObjectPart = obj,
                        BboxRadius = radius,
                        CogZ = cogZ
                    };

                    UpdateModelParameters(bbox, partObject, modelParameters);

                    partObjects.Add(partObject);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            return (partObjects, modelParameters);
        }
        //public bboxOverlap
    }
}
