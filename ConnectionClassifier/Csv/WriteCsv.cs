using ConnectionClassifier.GeometryCalculations;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GeometRi;

namespace ConnectionClassifier.Csv
{
    internal class WriteCsv
    {
        public static string GetXyzString(Point3d point)
        {
            return $"{point.X},{point.Y},{point.Z}";
        }

        public static string AddPlaneParameters(PlaneParameters planeParameters, string line)
        {
            line += $"{planeParameters.xStart};";
            line += $"{planeParameters.xOverlap};";
            line += $"{planeParameters.xEnd};";
            line += $"{planeParameters.yStart};";
            line += $"{planeParameters.yOverlap};";
            line += $"{planeParameters.yEnd};";
            line += $"{planeParameters.zStart};";
            line += $"{planeParameters.zOverlap};";
            line += $"{planeParameters.zEnd};";

            return line;
        }

        public static string AddConnectionAngles(ConnectionAnglesClass connectionAngles, string line)
        {
            line += $"{connectionAngles.AngleXX};";
            line += $"{connectionAngles.AngleXY};";
            line += $"{connectionAngles.AngleXZ};";
            line += $"{connectionAngles.AngleYX};";
            line += $"{connectionAngles.AngleYX};";
            line += $"{connectionAngles.AngleYZ};";
            line += $"{connectionAngles.AngleZX};";
            line += $"{connectionAngles.AngleZY};";
            line += $"{connectionAngles.AngleZZ};";

            return line;
        }

        public static string AddPartInformation(PartObject part, string line)
        {
            line += $"{part.GUID};";
            line += $"{part.Profile};";
            line += $"{part.Material};";
            line += $"{part.TeklaClass};";
            line += $"{part.Length};";
            line += $"{part.Height};";
            line += $"{part.Width};";
            line += $"{part.CogZ};";

            return line;
        }

        public static void WriteClassification(List<ConnectionObject> connectionObjects)
        {
            string date = DateTime.Now.ToString().Replace(':', '_').Replace('.', '_');

            // Prompt user to choose the save location
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            saveFileDialog.DefaultExt = "csv";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = $"Classification result {date}";

            if (saveFileDialog.ShowDialog() == true)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    writer.WriteLine("TYPE;GUID1;GUID2;PLANE 1 START;PLANE 1 END;PLANE 2 START;PLANE 2 END;" +
                        "PLANE1.xStart;PLANE1.xOverlap;PLANE1.xEnd;PLANE1.yStart;PLANE1.yOverlap;PLANE1.yEnd;PLANE1.zStart;PLANE1.zOverlap;PLANE1.zEnd;" +
                        "PLANE2.xStart;PLANE2.xOverlap;PLANE2.xEnd;PLANE2.yStart;PLANE2.yOverlap;PLANE2.yEnd;PLANE2.zStart;PLANE2.zOverlap;PLANE2.zEnd;" +
                        "ANGLE XX;ANGLE XY;ANGLE XZ;ANGLE YX;ANGLE YY;ANGLE YZ;ANGLE ZX;ANGLE ZY;ANGLE ZZ;" +
                        "PART1.GUID;PART1.PROFILE;PART1.MATERIAL;PART1.TeklaClass;PART1.LENGTH;PART1.HEIGHT;PART1.WIDTH;PART1.CogZ;" +
                        "PART2.GUID;PART2.PROFILE;PART2.MATERIAL;PART2.TeklaClass;PART2.LENGTH;PART2.HEIGHT;PART2.WIDTH;PART2.CogZ;");

                    foreach(ConnectionObject connectionObject in connectionObjects)
                    {
                        string guid1 = connectionObject.Part1.GUID;
                        string guid2 = connectionObject.Part2.GUID;
                        string type = connectionObject.ConnetionType;
                        string plane1Start = GetXyzString(connectionObject.Plane1Parameters.ConnectionStartPoint);
                        string plane1End = GetXyzString(connectionObject.Plane1Parameters.ConnectionEndPoint);
                        string plane2Start = GetXyzString(connectionObject.Plane2Parameters.ConnectionStartPoint);
                        string plane2End = GetXyzString(connectionObject.Plane2Parameters.ConnectionEndPoint);

                        string line = $"{type};{guid1};{guid2};{plane1Start};{plane1End};{plane2Start};{plane2End};";

                        line = AddPlaneParameters(connectionObject.Plane1Parameters, line);
                        line = AddPlaneParameters(connectionObject.Plane2Parameters, line);
                        line = AddConnectionAngles(connectionObject.ConnectionAngles, line);
                        line = AddPartInformation(connectionObject.Part1, line);
                        line = AddPartInformation(connectionObject.Part2, line);

                        writer.WriteLine(line);
                    }


                }
            }
        }
    }
}
