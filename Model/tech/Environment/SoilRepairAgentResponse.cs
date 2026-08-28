using Model.Base;

namespace Model.Tech.Environment
{
    public class SoilRepairAgentResponse : BaseOperateRight
    {
        public SoilRepairAgentResponse()
        {
            RepairForms = new List<DyDicColumnExtra>();
            RepairEfficiencys = new List<DyDicColumn>();
            Researchers = new List<string>();
        }

        public int ID { set; get; }

        public string RepairAgentNo { set; get; }

        public string RepairAgentSecondNo { set; get; }

        public decimal? HandleTime { set; get; }

        /// <summary>
        /// 固液比
        /// </summary>
        public string GYratio { set; get; }
        public string GYratio1 { set; get; }
        public string GYratio2 { set; get; }

        public decimal? PH { set; get; }

        public decimal? Voltage { set; get; }

        public decimal? Temperature { set; get; }

        public string ResearchTime { set; get; }


        public int? SoilRepairTechID { set; get; }

        public string SoilRepairTech { set; get; }

        public string RepairAgentSourceSort { set; get; }

        public string RepairAgentAssFile { set; get; }

        public string AssProjectIDs { set; get; }

        public int? DataRightValue { set; get; }
        public string DataRight { set; get; }

        public string Time { set; get; }
        public string Time1 { set; get; }
        public string Time2 { set; get; }

        public DateTime? CreateTime { set; get; }
        public string CreateUser { set; get; }

        public string CreateUserCode { set; get; }


        public List<DyDicColumnExtra> RepairForms { set; get; }

        public List<DyDicColumn> RepairEfficiencys { set; get; }

        public decimal? SortValue { set; get; }

        public List<string> Researchers { set; get; }
    }
}
