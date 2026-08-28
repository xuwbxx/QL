namespace Model.Tech.Cloud
{
    public class CloudWindTaskFile
    {
        public CloudWindTaskFile()
        {
            ExportFiles = new List<CloudProjectFile>();
            ImportFiles = new List<CloudProjectFile>();
        }

        public string TaskCode { set; get; }

        public string TaskName1 { set; get; }
        public string TaskName2 { set; get; }
        public int TypeID { set; get; }
        public string Type { set; get; }

        public string TaskTime { set; get; }

        public List<CloudProjectFile> ExportFiles { set; get; }

        public List<CloudProjectFile> ImportFiles { set; get; }
    }
}
