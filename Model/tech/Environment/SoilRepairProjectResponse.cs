using Model.Base;

namespace Model.Tech.Environment
{
    public class SoilRepairProjectResponse : BaseOperateRight
    {
        public int ID { set; get; }

        public string ProjectName { set; get; }

        public string ProjectChildName { set; get; }

        public string ProjectAddress { set; get; }

        public string Province { set; get; }

        public string City { set; get; }

        public string District { set; get; }

        public string RepairAgentNo { set; get; }

        public string RepairAgentSecondNo { set; get; }

        public int RepairAgentID { set; get; }

        public string RepairSoilAmount { set; get; }

        public string RepairPeriod { set; get; }

        public string RepairStandardName { set; get; }
        public string ArriveStandardName { set; get; }

        public string DirectCompany { set; get; }

        public string Director { set; get; }

        public DateTime StartTime { set; get; }

        public DateTime EndTime { set; get; }

        public string StartTimeStr { set; get; }

        public string EndTimeStr { set; get; }

        public string EndReport { set; get; }

        public string CreateUser { set; get; }

        public DateTime CreateTime { set; get; }

        public string CreateTimeStr { set; get; }



    }
}
