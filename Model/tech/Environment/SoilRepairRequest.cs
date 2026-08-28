using Model.Base;

namespace Model.Tech.Environment
{
    public class SoilRepairRequest : BaseRequest
    {
        public SoilRepairRequest()
        {
            QCons = new List<QueryCondition>();
            SoilPolluteInfo = new SoilPolluteInfoRequest();
            RepairProjectInfo = new RepairProjectInfoRequest();
            SoilRepairInfo = new SoilRepairInfoRequest();
            DictionaryInfo = new DictionaryRequest();
            FileInfo = new FileRequest();
            ManageInfo = new ManageRequest();
            RepairTechInfo = new RepairTechInfoRequest();


        }

        /// <summary>
        /// 1:新增 2：编辑
        /// </summary>
        public int OperateType { set; get; }
        public int ID { set; get; }

        public int TypeID { set; get; }

        public string StartTime { set; get; }

        public string EndTime { set; get; }

        public string RealName { set; get; }

        public int ProvinceID { set; get; }

        public string SoilRepairTech { set; get; }

        public string RepairAgentNo { set; get; }

        public string RepairAgentSecondNo { set; get; }

        public List<QueryCondition> QCons { set; get; }

        public SoilPolluteInfoRequest SoilPolluteInfo { set; get; }

        public RepairProjectInfoRequest RepairProjectInfo { set; get; }


        public SoilRepairInfoRequest SoilRepairInfo { set; get; }

        public RepairTechInfoRequest RepairTechInfo { set; get; }

        public DictionaryRequest DictionaryInfo { set; get; }

        public FileRequest FileInfo { set; get; }

        public ManageRequest ManageInfo { set; get; }

        /// <summary>
        /// 批量下载ID
        /// </summary>
        public string AgentIDs { set; get; }
    }

    public class SoilPolluteInfoRequest
    {
        public SoilPolluteInfoRequest()
        {
            SoilMetals = new List<SoilRepairDicInfo>();
            SoilPropertys = new List<SoilRepairDicInfo>();
            SoilPropertyStrs = new List<SoilRepairDicInfo>();
        }

        public int ID { set; get; }

        public string SoilSampleSource { set; get; }

        public string SoilType { set; get; }

        public string RepairAgentNo { set; get; }

        public int ProvinceID { set; get; }

        public List<SoilRepairDicInfo> SoilMetals { set; get; }

        public List<SoilRepairDicInfo> SoilPropertys { set; get; }

        public List<SoilRepairDicInfo> SoilPropertyStrs { set; get; }
    }

    public class SoilRepairDicInfo
    {
        public int ID { set; get; }

        public int DicID { set; get; }

        public string Value { set; get; }
    }

    public class RepairProjectInfoRequest
    {
        public RepairProjectInfoRequest()
        {

        }

        public int OperateType { set; get; }
        public int ID { set; get; }

        public string ProjectName { set; get; }

        public string ProjectChildName { set; get; }

        public string ProjectAddress { set; get; }

        public string Province { set; get; }

        public string City { set; get; }

        public string District { set; get; }

        public int RepairAgentID { set; get; }
        public string RepairAgentNo { set; get; }

        public string RepairAgentSecondNo { set; get; }

        public decimal? RepairSoilAmount { set; get; }

        public string RepairSoilAmountMin { set; get; }

        public string RepairSoilAmountMax { set; get; }

        public int? RepairPeriod { set; get; }

        public string RepairPeriodMin { set; get; }

        public string RepairPeriodMax { set; get; }

        public string RepairStandardName { set; get; }

        public string ArriveStandardName { set; get; }

        public string DirectCompany { set; get; }
        public string Director { set; get; }
        public string StartTime { set; get; }
        public string EndTime { set; get; }



    }

    public class SoilRepairInfoRequest
    {
        public SoilRepairInfoRequest()
        {
            RepairForms = new List<SoilRepairDicInfo>();
            RepairEfficiencys = new List<SoilRepairDicInfo>();
        }

        public int OperateType { set; get; }
        public int ID { set; get; }

        public string RepairAgentNo { set; get; }

        public string RepairAgentSecondNo { set; get; }

        public int RepairTech { set; get; }

        public string SourceSort { set; get; }

        public string ResearchTime { set; get; }


        public decimal? HandleTime { set; get; }

        public string HandleTimeMin { set; get; }

        public string HandleTimeMax { set; get; }

        public string GYratio { set; get; }

        public decimal? PH { set; get; }
        public decimal? Voltage { set; get; }

        public decimal? Temperature { set; get; }

        public int DataRight { set; get; }

        public string RepairFormsStr { set; get; }

        public string RepairEfficiencysStr { set; get; }

        public string Researcher { set; get; }

        public List<SoilRepairDicInfo> RepairForms { set; get; }

        public List<SoilRepairDicInfo> RepairEfficiencys { set; get; }
    }

    public class RepairTechInfoRequest
    {
        public int ID { set; get; }

        public string SoilRepairTech { set; get; }

        public string TechPrinciple { set; get; }

        public string TechFeature { set; get; }

        public string UseRange { set; get; }
    }

    public class DictionaryRequest
    {
        public int ID { set; get; }

        public int TypeID { set; get; }

        public int DataType { set; get; }

        public string Name { set; get; }

        public string Unit { set; get; }
    }

    public class FileRequest
    {
        public int ID { set; get; }

        public string FileName { set; get; }

        public int Type { set; get; }
    }


    public class ManageRequest
    {
        public int ID { set; get; }

        public string Name { set; get; }

        public int RoleID { set; get; }

        public int DataRight { set; get; }

        public int ViewRight { set; get; }

        public string Password { set; get; }

        public string UserCode { set; get; }

        public string Phone { set; get; }
    }


    public class ComQueryRequest : BaseRequest
    {
        public ComQueryRequest()
        {
            DataType = new List<string>();

        }
        public List<string> DataType { set; get; }

        public List<ComQueryString> MetalCons { set; get; }
    }

    public class ComQueryNumber
    {
        public int DicID { set; get; }

        public int DataType { set; get; }

        public double Value1 { set; get; }

        public double Value2 { set; get; }
    }

    public class ComQueryString
    {
        public int DicID { set; get; }

        public int DataType { set; get; }

        public double Value1 { set; get; }

        public double Value2 { set; get; }
    }
}
