namespace Model.Tech.Cloud
{
    public class CloudWindTaskDK
    {
        public CloudWindTaskDK()
        {
            File = new CloudProjectFile();
            DKs = new List<CloudWindTaskDKData>();
        }

        public CloudProjectFile File { set; get; }


        public List<CloudWindTaskDKData> DKs { set; get; }
    }

    public class CloudWindTaskDKData
    {
        public int ID { set; get; }

        public string DKName { set; get; }

        public string FilePath { set; get; }
    }



}
