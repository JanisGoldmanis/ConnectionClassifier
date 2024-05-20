using GeometRi;

namespace ConnectionClassifier.GeometryCalculations
{
    public class PartObject
    {
        public Box3d BBOX { get; set; }
        public DomainsClass Domains { get; set; }
        public string GUID { get; set; }
        public string Profile { get; set; }
        public string Material { get; set; }
        public string TeklaClass { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public Coord3d CS { get; set; }
        public Tekla.Structures.Model.Part modelObjectPart { get; set; }
        public double BboxRadius { get; set; }
        public double CogZ { get; set; }
    }

    public class DomainsClass
    {
        public double StartX { get; set; }
        public double EndX { get; set; }
        public double StartY { get; set; }
        public double EndY { get; set; }
        public double StartZ { get; set; }
        public double EndZ { get; set; }
    }
}
