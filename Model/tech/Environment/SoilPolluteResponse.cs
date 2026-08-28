using Model.Base;

namespace Model.Tech.Environment
{
    public class SoilPolluteResponse : BaseOperateRight
    {
        public SoilPolluteResponse()
        {
            SoilMetals = new List<DyDicColumn>();
            SoilPropertys = new List<DyDicColumn>();
            SoilPropertyStrs = new List<DyDicStrColumn>();
        }

        public int ID { set; get; }
        public List<DyDicColumn> SoilMetals { set; get; }

        public string SoilSampleSource { set; get; }

        public string SoilType { set; get; }

        public List<DyDicColumn> SoilPropertys { set; get; }
        public List<DyDicStrColumn> SoilPropertyStrs { set; get; }

        public string RepairAgentNo { set; get; }

        public int ProvinceID { set; get; }
        public string Province { set; get; }
        public string Time { set; get; }
        public string Time1 { set; get; }
        public string Time2 { set; get; }

        public DateTime? CreateTime { set; get; }
        public string CreateUser { set; get; }

        public string CreateUserCode { set; get; }

        public decimal? SortValue { set; get; }
    }
}
