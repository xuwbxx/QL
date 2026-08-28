using Model.Base;

namespace Model.Tech.Environment
{
    public class SoilRepairTechResponse : BaseOperateRight
    {
        public int ID { set; get; }

        public string SoilRepairTech { set; get; }

        public string TechPrinciple { set; get; }

        public string TechFeature { set; get; }

        public string UseRange { set; get; }

        public string CreateTime { set; get; }

        public string CreateUser { set; get; }
    }
}
