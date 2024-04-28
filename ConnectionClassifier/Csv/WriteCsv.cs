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
                    writer.WriteLine("TYPE;GUID1;GUID2;PLANE 1 START;PLANE 1 END;PLANE 2 START;PLANE 2 END");

                    foreach(ConnectionObject connectionObject in connectionObjects)
                    {
                        string guid1 = connectionObject.Part1.GUID;
                        string guid2 = connectionObject.Part2.GUID;
                        string type = connectionObject.ConnetionType;
                        string plane1Start = GetXyzString(connectionObject.Plane1Parameters.ConnectionStartPoint);
                        string plane1End = GetXyzString(connectionObject.Plane1Parameters.ConnectionEndPoint);
                        string plane2Start = GetXyzString(connectionObject.Plane2Parameters.ConnectionStartPoint);
                        string plane2End = GetXyzString(connectionObject.Plane2Parameters.ConnectionEndPoint);

                        string line = $"{type};{guid1};{guid2};{plane1Start};{plane1End};{plane2Start};{plane2End}";

                        writer.WriteLine(line);
                    }


                }
            }
        }
    }
}
