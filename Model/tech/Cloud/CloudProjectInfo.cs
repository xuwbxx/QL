namespace Model.Tech.Cloud
{
    public class CloudProjectInfo
    {
        public CloudProjectInfo()
        {
            Files = new List<CloudProjectFile>();
            Node = new CloudProjectNode();
        }

        public int ID { set; get; }

        public string ProjectName { set; get; }

        public string Lon { set; get; }

        public string Lat { set; get; }

        /// <summary>
        /// 0:未建  1：在建  2：已建
        /// </summary>
        public int Status { set; get; }



        public List<CloudProjectFile> Files { set; get; }

        public string ProjectManagerName { set; get; }

        public string ProjectManagerDepart { set; get; }

        public string ProjectManagerUserCode { set; get; }

        public string ProjectManagerPhone { set; get; }

        public string ProjectManagerJobName { set; get; }

        public string ProjectStartTime { set; get; }

        public string ProjectEndTime { set; get; }

        public CloudProjectNode Node { set; get; }

        public string LastComment { set; get; }

        public bool IsBackFlow { set; get; }
    }

    public class CloudProjectNode
    {
        public int ID { set; get; }

        public string NodeName { set; get; }
    }

    public class CloudProjectFanInfo
    {
        public int ID { set; get; }

        public string FanName { set; get; }

        public string Lon { set; get; }

        public string Lat { set; get; }

        /// <summary>
        /// 0:未安装  1：正在安装  2：已安装
        /// </summary>
        public int Status { set; get; }
    }

    public class CloudProjectPosition
    {
        public string Lon { set; get; }
        public string Lat { set; get; }

    }

    public class CloudProjectFanPosition
    {
        public string FanName { set; get; }
        public string Lon { set; get; }
        public string Lat { set; get; }

    }

    public class CloudProjectFileType
    {
        public CloudProjectFileType()
        {
            Geology_yszl = new List<CloudProjectFile>();
            Geology_sjb = new List<CloudProjectFile>();
        }

        public List<CloudProjectFile> Geology_yszl { set; get; }

        public List<CloudProjectFile> Geology_sjb { set; get; }
    }


    public class CloudProjectFile
    {
        public int ID { set; get; }
        public string FileName { set; get; }

        public string FileLength { set; get; }

        public string FileExtension { set; get; }

        public string FilePath { set; get; }

        public string FileTime { set; get; }

        public string FileType { set; get; }
    }
}
