using ConnectionClassifier.Csv;
using ConnectionClassifier.GeometryCalculations;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Solid;
using TSG = Tekla.Structures.Geometry3d;


namespace ConnectionClassifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string ClassValue { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            ToleranceTextBox.Text = "200";

            if (Debugger.IsAttached)
            {
                ConfigLocationTextBox.Text = "C:\\Users\\janis.goldmanis\\Downloads\\ConnectionClassificationConfig\\config.json";
                ListsLocationTextBox.Text = "C:\\Users\\janis.goldmanis\\Downloads\\ConnectionClassificationConfig\\lists.json";
            }
        }
      
        public static List<int>[][][] GetModelLists(ModelParameters modelParameters, List<PartObject> partObjects, double tolerance)
        {
            double maxXSize = modelParameters.maxXLength + tolerance;
            double xLength = modelParameters.maxX - modelParameters.minX;
            double maxYSize = modelParameters.maxYLength + tolerance;
            double yLength = modelParameters.maxY - modelParameters.minY;
            double maxZSize = modelParameters.maxZLength + tolerance;
            double zLength = modelParameters.maxZ - modelParameters.minZ;

            int xCount = (int)Math.Ceiling(xLength / maxXSize);
            int yCount = (int)Math.Ceiling(yLength / maxYSize);
            int zCount = (int)Math.Ceiling(zLength / maxZSize);

            List<int>[][][] data = new List<int>[partObjects.Count][][];
            return data;
        }

        public void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {


                DateTime startTime = DateTime.Now;
                int clashTolerance = int.Parse(ToleranceTextBox.Text);


                var bbox = new BoundingBox();
                (List<PartObject> partObjects, ModelParameters modelParameters) = bbox.GetClassificationPartObjects();

                var data = GetModelLists(modelParameters, partObjects, clashTolerance);

                DateTime objectsCreatedTime = DateTime.Now;

                var clashDetection = new Clash();

                int roundingTolerance = 1;

                List<ConnectionObject> connectionObjects = clashDetection.ClashDetectionBruteForce(partObjects, clashTolerance, roundingTolerance);

                DateTime endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime;
                TimeSpan createdObjectsSpan = objectsCreatedTime - startTime;

                var configLocation = ConfigLocationTextBox.Text;
                var jsonListsLocation = ListsLocationTextBox.Text;

                var json = ParseJson.ParseJson.ReadJsonFile(configLocation);
                var jsonLists = ParseJson.ParseJson.ReadJsonFile(jsonListsLocation)["lists"];

                var jsonParser = new ParseJson.ParseJson();

                Dictionary<string, int> typeCount = new Dictionary<string, int>();

                foreach (var connectionObject in connectionObjects)
                {
                    try
                    {
                        jsonParser.ClassifyConnection(connectionObject, json, jsonLists, roundingTolerance);
                        string type = connectionObject.ConnetionType;

                        if (!typeCount.ContainsKey(type))
                        {
                            typeCount[type] = 0;
                        }
                        typeCount[type] += 1;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }

                WriteCsv.WriteClassification(connectionObjects);

                bboxClashIter.Content = $"Calculated Bboxes: {createdObjectsSpan.ToString()}\nBrute Force clashes: {duration.ToString()}\n";

                var test = json[0]["Name"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                ConfigLocationTextBox.Text = selectedFileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                ListsLocationTextBox.Text = selectedFileName;
            }
        }
    }
}
