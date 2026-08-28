namespace Model.Tech.Cloud
{
    public class CloudWindTaskShip
    {
        public CloudWindTaskShip()
        {
            Ships = new List<CloudWindTaskShipData>();
            Files = new List<CloudWindTaskShipFile>();
        }
        public List<CloudWindTaskShipData> Ships { set; get; }

        public List<CloudWindTaskShipFile> Files { set; get; }
    }

    public class CloudWindTaskShipData
    {
        public int ID { set; get; }

        public string ShipName { set; get; }

    }

    public class CloudWindTaskShipFile
    {
        public int ID { set; get; }

        public string FileName { set; get; }

        public string FilePath { set; get; }
    }
}
