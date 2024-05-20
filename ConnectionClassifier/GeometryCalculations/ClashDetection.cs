using GeometRi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.ModelInternal;

namespace ConnectionClassifier.GeometryCalculations
{
    public class Clash
    {
        public Plane3d GetPlane(string planeType, PartObject partObject)
        {
            switch(planeType)
            {
                case "XY":
                    return partObject.CS.XY_plane;
                case "XZ":
                    return partObject.CS.XZ_plane;
                case "YZ":
                    return partObject.CS.YZ_plane;
                default:
                    throw new ArgumentException("Invalid plane type");
            }
        }

        public double GetPoint3dVectorValue(string vector, Point3d point)
        {
            switch(vector)
            {
                case "X": return point.X;
                case "Y": return point.Y;
                case "Z": return point.Z;
                default: throw new ArgumentException("Invalid direction");
            }
        }


        public bool TwoPartObjectsClash(PartObject mainPart, PartObject secondaryObject, int clashTolerance)
        {
            //Check mainPart XY plane
            var mainPointList = mainPart.BBOX.ListOfPoints;
            var secondaryPointList = secondaryObject.BBOX.ListOfPoints;

            var point1 = mainPart.BBOX.Center;
            var point2 = secondaryObject.BBOX.Center;

            double distance = point1.DistanceTo(point2);

            double radius1 = mainPart.BboxRadius;
            double radius2 = secondaryObject.BboxRadius;

            if (distance > radius1 + radius2 + clashTolerance)
            {
                return false;
            }

            string[] planeTypes = new string[]{ "XY", "XZ", "YZ" };

            List<string[]> mainVectors = new List<string[]>();
            mainVectors.Add(new string[] { "X", "Y" });
            mainVectors.Add(new string[] { "X", "Z" });
            mainVectors.Add(new string[] { "Y", "Z" });

            int overlappingPlanes = 0;

            var pointDrawer = new DrawObjectsInTekla();

            var coordinateSystem = mainPart.CS;

            for (int planeIndex = 0; planeIndex <=2; planeIndex++)
            {
                string planeType = planeTypes[planeIndex];
                string[] mainVectorDirections = mainVectors[planeIndex];

                string firstVector = mainVectorDirections[0];
                string secondVector = mainVectorDirections[1];

                Plane3d plane = GetPlane(planeType, mainPart);

                double mainMinFirstVector = double.MaxValue;
                double mainMaxFirstVector = double.MinValue;
                double mainMinSecondVector = double.MaxValue;
                double mainMaxSecondVector = double.MinValue;

                double secondaryMinFirstVector = double.MaxValue;
                double secondaryMaxFirstVector = double.MinValue;
                double secondaryMinSecondVector = double.MaxValue;
                double secondaryMaxSecondVector = double.MinValue;

                foreach (Point3d point in mainPointList)
                {
                    Point3d projectedPoint = point.ProjectionTo(plane);

                    //pointDrawer.DrawPoint3d(projectedPoint);

                    projectedPoint = projectedPoint.ConvertTo(coordinateSystem);

                    double projectedPointFirstVectorCoordinate = GetPoint3dVectorValue(firstVector, projectedPoint);
                    if (projectedPointFirstVectorCoordinate < mainMinFirstVector)
                    {
                        mainMinFirstVector = projectedPointFirstVectorCoordinate;
                    }
                    if (projectedPointFirstVectorCoordinate > mainMaxFirstVector)
                    {
                        mainMaxFirstVector = projectedPointFirstVectorCoordinate;
                    }

                    double projectedPointSecondVectorCoordinate = GetPoint3dVectorValue(secondVector, projectedPoint);
                    if (projectedPointSecondVectorCoordinate < mainMinSecondVector)
                    {
                        mainMinSecondVector = projectedPointSecondVectorCoordinate;
                    }
                    if (projectedPointSecondVectorCoordinate > mainMaxSecondVector)
                    {
                        mainMaxSecondVector = projectedPointSecondVectorCoordinate;
                    }
                }

                foreach (Point3d point in secondaryPointList)
                {
                    Point3d projectedPoint = point.ProjectionTo(plane);
                    //pointDrawer.DrawPoint3d(projectedPoint);

                    projectedPoint = projectedPoint.ConvertTo(coordinateSystem);

                    double projectedPointFirstVectorCoordinate = GetPoint3dVectorValue(firstVector, projectedPoint);

                    if (projectedPointFirstVectorCoordinate < secondaryMinFirstVector)
                    {
                        secondaryMinFirstVector = projectedPointFirstVectorCoordinate;
                    }
                    if (projectedPointFirstVectorCoordinate > secondaryMaxFirstVector)
                    {
                        secondaryMaxFirstVector = projectedPointFirstVectorCoordinate;
                    }

                    double projectedPointSecondVectorCoordinate = GetPoint3dVectorValue(secondVector, projectedPoint);
                    if (projectedPointSecondVectorCoordinate < secondaryMinSecondVector)
                    {
                        secondaryMinSecondVector = projectedPointSecondVectorCoordinate;
                    }
                    if (projectedPointSecondVectorCoordinate > secondaryMaxSecondVector)
                    {
                        secondaryMaxSecondVector = projectedPointSecondVectorCoordinate;
                    }
                }

                mainMinFirstVector -= clashTolerance;
                mainMaxFirstVector += clashTolerance;
                mainMinSecondVector -= clashTolerance;
                mainMaxSecondVector += clashTolerance;

                secondaryMinFirstVector -= clashTolerance;
                secondaryMaxFirstVector += clashTolerance;
                secondaryMinSecondVector -= clashTolerance;
                secondaryMaxSecondVector += clashTolerance;

                bool x = false;
                if (mainMaxFirstVector <= secondaryMaxFirstVector && mainMaxFirstVector >= secondaryMinFirstVector)
                {
                    x = true;
                }

                if (secondaryMaxFirstVector <= mainMaxFirstVector && secondaryMaxFirstVector >= mainMinFirstVector)
                {
                    x = true;
                }
                if (!x)
                {
                    return false;
                }

                bool y = false;
                if (mainMaxSecondVector <= secondaryMaxSecondVector && mainMaxSecondVector >= secondaryMinSecondVector)
                {
                    y = true;
                }

                if (secondaryMaxSecondVector <= mainMaxSecondVector && secondaryMaxSecondVector >= mainMinSecondVector)
                {
                    y = true;
                }    

                if (!y)
                {
                    return false;
                }
                overlappingPlanes++;
            }
            if (overlappingPlanes == 3)
            {
                return true;
            }
            return false;
        }


        public List<ConnectionObject> ClashDetectionBruteForce(List<PartObject> partObjectsSource, int clashTolerance, int roundingTolerance) 
        {
            List<PartObject> partObjects = new List<PartObject>(partObjectsSource);

            List<List<int>> clashIndexes = new List<List<int>>();

            List<ConnectionObject> connectionObjects = new List<ConnectionObject>();

            ConnectionParameters connectionParameter = new ConnectionParameters();

            int i = 0;

            while (partObjects.Count > 1) 
            {
                i++;
                PartObject mainObject = partObjects.First();
                partObjects.RemoveAt(0);

                foreach (PartObject secondaryObject in partObjects)
                {
                    //bool clashGeometri = mainObject.BBOX.Intersects(secondaryObject.BBOX);
                    bool clash1 = TwoPartObjectsClash(mainObject, secondaryObject, clashTolerance);
                    bool clash2 = TwoPartObjectsClash(secondaryObject, mainObject, clashTolerance);
                    if (clash1 && clash2)
                    //if (clashGeometri)
                    {
                        ConnectionObject connectionObject = new ConnectionObject()
                        {
                            Part1 = mainObject,
                            Part2 = secondaryObject,
                            Plane1Parameters = new PlaneParameters(),
                            Plane2Parameters = new PlaneParameters(),
                            ConnectionAngles = new ConnectionAnglesClass(),
                            ConnetionType = ""                            
                        };

                        connectionParameter.CalculateConnectionParameters(connectionObject, roundingTolerance);
                        connectionObjects.Add(connectionObject);

                    }
                }
                Trace.WriteLine(partObjects.Count);

                if(i == 10000)
                {

                }
            }
            return connectionObjects;
        }


        public List<ConnectionObject> ClashDetection(List<PartObject> partObjectsSource, int clashTolerance, int roundingTolerance)
        {
            List<PartObject> partObjects = new List<PartObject>(partObjectsSource);

            List<ConnectionObject> connectionObjects = new List<ConnectionObject>();

            ConnectionParameters connectionParameter = new ConnectionParameters();

            while (partObjects.Count > 1)
            {
                PartObject mainObject = partObjects.First();
                partObjects.RemoveAt(0);


                foreach (PartObject secondaryObject in partObjects)
                {
                    if (secondaryObject.Domains.StartX > mainObject.Domains.EndX)
                    {
                        break;
                    }

                    bool clash1 = TwoPartObjectsClash(mainObject, secondaryObject, clashTolerance);
                    bool clash2 = TwoPartObjectsClash(secondaryObject, mainObject, clashTolerance);
                    if (clash1 && clash2)
                    {
                        ConnectionObject connectionObject = new ConnectionObject()
                        {
                            Part1 = mainObject,
                            Part2 = secondaryObject,
                            Plane1Parameters = new PlaneParameters(),
                            Plane2Parameters = new PlaneParameters(),
                            ConnectionAngles = new ConnectionAnglesClass(),
                            ConnetionType = ""
                        };

                        connectionParameter.CalculateConnectionParameters(connectionObject, roundingTolerance);
                        connectionObjects.Add(connectionObject);

                    }
                }
                
            }
            return connectionObjects;
        }
    }
}
