namespace Model.Tech.Cloud.Webgis
{
    public class GisProjectMain
    {
        public GisProjectMain()
        {
            Fans = new List<GisFan>();
            Areas = new List<GisProjectArea>();
        }
        public int ID { set; get; }

        public string ProjectName { set; get; }

        public string Lon { set; get; }

        public string Lat { set; get; }

        public int Status { set; get; }

        public string StatusName { set; get; }
        public List<GisProjectArea> Areas { set; get; }

        public List<GisFan> Fans { set; get; }
    }

    public class GisProject
    {
        public GisProject()
        {
            Fans = new List<GisFan>();
            Areas = new List<GisProjectArea>();
        }
        public int ID { set; get; }

        public string ProjectName { set; get; }

        public int Status { set; get; }

        public string Lon { set; get; }

        public string Lat { set; get; }

        public List<GisFan> Fans { set; get; }

        public List<GisProjectArea> Areas { set; get; }
    }

    public class GisFan
    {
        public int ID { set; get; }

        public string FanName { set; get; }

        public int Status { set; get; }

        public string StatusName { set; get; }

        public string Lon { set; get; }

        public string Lat { set; get; }
    }

    public class GisProjectArea
    {
        public int ID { set; get; }

        public string Lon { set; get; }

        public string Lat { set; get; }
    }
}
