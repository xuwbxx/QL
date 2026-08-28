namespace Model.Tech.Cloud.Webgis
{
    public class PXYShipAnchor
    {
        public PXYShipAnchor()
        {
            Positions = new List<ShipAnchorPosition>();
        }
        public string ID { set; get; }
        public string AnchorName { set; get; }

        public string ProjectName { set; get; }

        public List<ShipAnchorPosition> Positions { set; get; }
    }

    public class ShipAnchorPosition
    {
        public string Lon { set; get; }

        public double LonValue { set; get; }

        public string Lat { set; get; }

        public double LatValue { set; get; }
    }
}
