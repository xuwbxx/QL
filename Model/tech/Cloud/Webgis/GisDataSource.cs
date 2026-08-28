namespace Model.Tech.Cloud.Webgis
{
    public class GisDataSource
    {
        public GisDataSource()
        {
            DKList = new List<DKModel>();
            PileList = new List<PileSourceData>();
        }

        public List<DKModel> DKList { set; get; }

        public List<PileSourceData> PileList { set; get; }
    }
}
