namespace Model.TechCenter
{
    public class TC_DepartInfo
    {
        public TC_DepartInfo()
        {
            Softwares = new List<TC_DepartSoftware>();
        }

        public int DepartID { set; get; }

        public string? DepartName { set; get; }

        public List<TC_DepartSoftware> Softwares { set; get; }

    }

    public class TC_DepartSoftware
    {
        public int SoftwareID { set; get; }

        public string? SoftwareName { set; get; }

        public string? Img { set; get; }

        public string? Info { set; get; }

        public string? Manager { set; get; }

        public string? UseTime { set; get; }

    }

}
