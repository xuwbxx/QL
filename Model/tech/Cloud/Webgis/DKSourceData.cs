namespace Model.Tech.Cloud.Webgis
{
    public class DKSourceData
    {

    }

    public class DKModel
    {
        public DKModel()
        {
            Data = new List<DKData>();
        }
        public string DKName { set; get; }

        public List<DKData> Data { set; get; }
    }

    public class DKData
    {
        public string 序号 { set; get; }

        public string 地层编号 { set; get; }

        public string 土层名称 { set; get; }

        public string 层底标高_m { set; get; }
        public string 土层类型 { set; get; }
        public string 不排水抗剪强度_cu_kPa { set; get; }
        public string 砂土摩擦角_度 { set; get; }
        public string 有效重度 { set; get; }
        public string 标贯击数 { set; get; }
    }

}
