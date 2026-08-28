namespace Model.Tech.Cloud
{
    public class CloudWindDKFile
    {
        public CloudWindDKFile()
        {
            File_dksjb = new List<CloudProjectFile>();
            File_dkyszl = new List<CloudProjectFile>();
        }

        public List<CloudProjectFile> File_dkyszl { set; get; }

        public List<CloudProjectFile> File_dksjb { set; get; }

    }
}
