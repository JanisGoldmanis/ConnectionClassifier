using GeometRi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using TSG = Tekla.Structures.Geometry3d;

namespace ConnectionClassifier.GeometryCalculations
{
    internal class DrawObjectsInTekla
    {
        public void DrawBBox(Box3d bbox)
        {
            var drawer = new GraphicsDrawer();
            foreach (GeometRi.Segment3d segment in bbox.ListOfEdges)
            {
                Point3d segmentStart = segment.P1.ConvertToGlobal();
                Point3d segmentEnd = segment.P2.ConvertToGlobal();

                TSG.Point start = new TSG.Point(segmentStart.X, segmentStart.Y, segmentStart.Z);
                TSG.Point end = new TSG.Point(segmentEnd.X, segmentEnd.Y, segmentEnd.Z);

                drawer.DrawLineSegment(start, end, new Tekla.Structures.Model.UI.Color(1, 0, 0));
            }
        }

        public void DrawPoint3d(Point3d point)
        {
            Point pointTekla = new Point(point.X, point.Y, point.Z);

            ControlPoint controlPoint = new ControlPoint(pointTekla);

            bool Result = false;

            Result = controlPoint.Insert();

            Model model = new Model();

            model.CommitChanges();
        }
    }


}
