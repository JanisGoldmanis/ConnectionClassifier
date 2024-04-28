using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeometRi;
using Tekla.Structures.ModelInternal;

namespace ConnectionClassifier.GeometryCalculations
{
    internal class ConnectionParameters
    {
        public void CalculateConnectionParameters(ConnectionObject connectionObject) 
        {
            PartObject part1 = connectionObject.Part1;
            PartObject part2 = connectionObject.Part2;

            Box3d box1 = part1.BBOX;
            Box3d box2 = part2.BBOX;

            CalculateOnPlane(box1, box2, connectionObject, connectionObject.Plane1Parameters);
            CalculateOnPlane(box2, box1, connectionObject, connectionObject.Plane2Parameters);
            CalculateConnectionAngles(box1, box2, connectionObject);
        }

        public void CalculateOnPlane(Box3d box1, Box3d box2, 
            ConnectionObject connectionObject, 
            PlaneParameters planeParameters) 
        {
            Coord3d coord3D = box1.LocalCoord();

            List<Point3d> points1 = box1.ListOfPoints;
            List<Point3d> points2 = box2.ListOfPoints;

            List<Point3d> convertedPoints1 = new List<Point3d>();

            foreach (Point3d point in points1)
            {
                Point3d convertedPoint = point.ConvertTo(coord3D);
                convertedPoints1.Add(convertedPoint);
            }

            List<Point3d> convertedPoints2 = new List<Point3d>();

            foreach(Point3d point in points2) 
            {
                Point3d convertedPoint = point.ConvertTo(coord3D);
                convertedPoints2.Add(convertedPoint);
            }

            double minX1 = double.MaxValue;
            double maxX1 = double.MinValue;
            double minY1 = double.MaxValue;
            double maxY1 = double.MinValue;
            double minZ1 = double.MaxValue;               
            double maxZ1 = double.MinValue;

            foreach (Point3d point in convertedPoints1)
            {
                minX1 = Math.Min(minX1, point.X);
                maxX1 = Math.Max(maxX1, point.X);
                minY1 = Math.Min(minY1, point.Y);
                maxY1 = Math.Max(maxY1, point.Y);
                minZ1 = Math.Min(minZ1, point.Z);
                maxZ1 = Math.Max(maxZ1, point.Z);
            }

            double minX2 = double.MaxValue;
            double maxX2 = double.MinValue;
            double minY2 = double.MaxValue;
            double maxY2 = double.MinValue;
            double minZ2 = double.MaxValue;
            double maxZ2 = double.MinValue;

            foreach(Point3d point in convertedPoints2)
            {
                minX2 = Math.Min(minX2, point.X);
                maxX2 = Math.Max(maxX2, point.X);
                minY2 = Math.Min(minY2, point.Y);
                maxY2 = Math.Max(maxY2, point.Y);
                minZ2 = Math.Min(minZ2, point.Z);
                maxZ2 = Math.Max(maxZ2, point.Z);
            }

            planeParameters.xStart = Math.Max(minX1, minX2) - Math.Min(minX1, minX2);
            planeParameters.xOverlap = Math.Min(maxX1, maxX2) - Math.Max(minX1, minX2);
            planeParameters.xEnd = Math.Max(maxX1, maxX2) - Math.Min(maxX1, maxX2);

            planeParameters.yStart = Math.Max(minY1, minY2) - Math.Min(minY1, minY2);
            planeParameters.yOverlap = Math.Min(maxY1, maxY2) - Math.Max(minY1, minY2);
            planeParameters.yEnd = Math.Max(maxY1, maxY2) - Math.Min(maxY1, maxY2);

            planeParameters.zStart = Math.Max(minZ1, minZ2) - Math.Min(minZ1, minZ2);
            planeParameters.zOverlap = Math.Min(maxZ1, maxZ2) - Math.Max(minZ1, minZ2);
            planeParameters.zEnd = Math.Max(maxZ1, maxZ2) - Math.Min(maxZ1, maxZ2);

            // Calculate bbox for bbox overlaps
            double xStart = Math.Max(minX1, minX2);
            double xEnd = Math.Min(maxX1, maxX2);
            double yStart = Math.Max(minY1, minY2);
            double yEnd = Math.Min(maxY1, maxY2);
            double zStart = Math.Max(minZ1, minZ2);
            double zEnd = Math.Min(maxZ1, maxZ2);

            DrawObjectsInTekla objectDrawer = new DrawObjectsInTekla();
            var point1 = new Point3d(xStart, yStart, zStart, coord3D);
            var point1global = point1.ConvertToGlobal();
            var point2 = new Point3d(xEnd, yEnd, zEnd, coord3D);
            var point2global = point2.ConvertToGlobal();

            //objectDrawer.DrawPoint3d(point1global);
            //objectDrawer.DrawPoint3d(point2global);

            var xLength = xEnd - xStart;
            var yLength = yEnd - yStart;
            var zLength = zEnd - zStart;

            double[] coordinates = new double[] { xLength, yLength, zLength };
            char[] coordinateChars = new char[] { 'x', 'y', 'z' };

            var largestLength = coordinates[0];
            var largestChar = coordinateChars[0];

            for (int i = 0; i < coordinates.Length; i++)
            {
                if (largestLength < coordinates[i])
                {
                    largestLength = coordinates[i];
                    largestChar = coordinateChars[i];
                }
            }

            double x1 = 0;
            double y1 = 0;
            double z1 = 0;
            double x2 = 0;
            double y2 = 0;
            double z2 = 0;

            if (largestChar == 'z')
            {
                z1 = zStart;
                z2 = zEnd;

                double xCoordinate = xStart + (xEnd - xStart) / 2;
                x1 = xCoordinate;
                x2 = xCoordinate;

                double yCoordinate = yStart + (yEnd - yStart) / 2;
                y1 = yCoordinate;
                y2 = yCoordinate;
            }
            if (largestChar == 'y')
            {
                y1 = yStart;
                y2 = yEnd;

                double xCoordinate = xStart + (xEnd - xStart) / 2;
                x1 = xCoordinate;
                x2 = xCoordinate;

                double zCoordinate = zStart + (zEnd - zStart) / 2;
                z1 = zCoordinate;
                z2 = zCoordinate;
            }
            if (largestChar == 'x')
            {
                x1 = xStart;
                x2 = xEnd;

                double yCoordinate = yStart + (yEnd - yStart) / 2;
                y1 = yCoordinate;
                y2 = yCoordinate;

                double zCoordinate = zStart + (zEnd - zStart) / 2;
                z1 = zCoordinate;
                z2 = zCoordinate;
            }

            Point3d point1Center = new Point3d(x1, y1, z1, coord3D);
            Point3d point1CenterGlobal = point1Center.ConvertToGlobal();
            Point3d point2Center = new Point3d(x2, y2, z2, coord3D);
            Point3d point2CenterGlobal = point2Center.ConvertToGlobal();

            planeParameters.ConnectionStartPoint = point1CenterGlobal;
            planeParameters.ConnectionEndPoint = point2CenterGlobal;

            //objectDrawer.DrawPoint3d(point1CenterGlobal);
            //objectDrawer.DrawPoint3d(point2CenterGlobal);
        }

        public void CalculateConnectionAngles(Box3d box1, Box3d box2, ConnectionObject connectionObject)
        {
            var x1 = box1.V1;
            var y1 = box1.V2;
            var z1 = box1.V3;

            var x2 = box2.V1;
            var y2 = box2.V2;
            var z2 = box2.V3;

            double XX = x1.AngleToDeg(x2);
            double XY = x1.AngleToDeg(y2);
            double XZ = x1.AngleToDeg(z2);

            double YX = y1.AngleToDeg(x2);
            double YY = y1.AngleToDeg(y2);
            double YZ = y1.AngleToDeg(z2);

            double ZX = z1.AngleToDeg(x2);
            double ZY = z1.AngleToDeg(y2);
            double ZZ = z1.AngleToDeg(z2);

            ConnectionAnglesClass connectionAngles = connectionObject.ConnectionAngles;

            connectionAngles.AngleXX = XX;
            connectionAngles.AngleXY = XY;
            connectionAngles.AngleXZ = XZ;
            connectionAngles.AngleYX = YX;
            connectionAngles.AngleYY = YY;
            connectionAngles.AngleYZ = YZ;
            connectionAngles.AngleZX = ZX;
            connectionAngles.AngleZY = ZY;
            connectionAngles.AngleZZ = ZZ;
        }
    }
}
