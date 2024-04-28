using ConnectionClassifier.Csv;
using ConnectionClassifier.GeometryCalculations;
using System;
using System.Collections;
using System.Collections.Generic;
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
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    Model model = new Model();

        //    if (!model.GetConnectionStatus())
        //    {
        //        MessageBox.Show("Tekla Structures not connected");
        //        return;
        //    }

        //    ModelInfo modelInfo = model.GetInfo();
        //    string name = modelInfo.ModelName;

        //    MessageBox.Show(string.Format("Hello world! Current model is named: {0}", name));

        //    Operation.DisplayPrompt(string.Format("Hello world! Current model is named: {0}", name));
        //}

        public void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {


                DateTime startTime = DateTime.Now;

                var bbox = new BoundingBox();
                List<PartObject> partObjects = bbox.GetClassificationPartObjects();

                DateTime objectsCreatedTime = DateTime.Now;

                var clashDetection = new Clash();


                //// Subscribe to the ClashProgress event
                //clashDetection.ClashProgress += ClashDetection_ClashProgress;

                int clashTolerance = 200;

                List<ConnectionObject> connectionObjects = clashDetection.ClashDetectionBruteForce(partObjects, clashTolerance);

                DateTime endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime;
                TimeSpan createdObjectsSpan = objectsCreatedTime - startTime;

                bboxClashIter.Content = $"{createdObjectsSpan.ToString()}\n{duration.ToString()}";

                var json = ParseJson.ParseJson.ReadJsonFile("config - HCS anchors on walls.json");
                var jsonLists = ParseJson.ParseJson.ReadJsonFile("lists.json")["lists"];

                var jsonParser = new ParseJson.ParseJson();

                int tolerance = 1;

                Dictionary<string, int> typeCount = new Dictionary<string, int>();

                foreach (var connectionObject in connectionObjects)
                {
                    jsonParser.ClassifyConnection(connectionObject, json, jsonLists, tolerance);
                    string type = connectionObject.ConnetionType;

                    if (!typeCount.ContainsKey(type))
                    {
                        typeCount[type] = 0;
                    }
                    typeCount[type] += 1;
                }

                WriteCsv.WriteClassification(connectionObjects);

                List<PartObject> sortedByXConnectionObjects = partObjects.OrderBy(o => o.Domains.StartX).ToList();



                var test = json[0]["Name"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
