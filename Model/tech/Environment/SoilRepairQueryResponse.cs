namespace Model.Tech.Environment
{
    public class SoilRepairQueryResponse
    {
        public SoilRepairQueryResponse()
        {
            QueryData = new List<SoilRepairQueryData>();
            QueryCount = new SoilRepairQueryCount();
        }

        public List<SoilRepairQueryData> QueryData { set; get; }

        public SoilRepairQueryCount QueryCount { set; get; }

        public string QueryParam { set; get; }

        public int DataCount { set; get; }
    }

    public class SoilRepairQueryData
    {
        public SoilRepairQueryData()
        {
            SoilMetals = new List<SoilRepairDicQuery>();
            SoilPros = new List<SoilRepairDicQuery>();
            RepairAgent = new SoilRepairAgentQuery();
        }

        public int ID { set; get; }

        public int DownloadCount { set; get; }

        public decimal? OrderValue { set; get; }

        public int ProvinceID { set; get; }

        public string Province { set; get; }

        public string CreateUser { set; get; }

        public DateTime CreateTime { set; get; }

        /// <summary>
        /// 土壤重金属
        /// </summary>
        public List<SoilRepairDicQuery> SoilMetals { set; get; }

        /// <summary>
        /// 土壤特性
        /// </summary>
        public List<SoilRepairDicQuery> SoilPros { set; get; }


        public SoilRepairAgentQuery RepairAgent { set; get; }

    }

    public class SoilRepairAgentQuery
    {
        public SoilRepairAgentQuery()
        {
            RepairFiles = new List<FileQuery>();
            RepairProjects = new List<SoilRepairAssProject>();

            RepairComNames = new List<SoilRepairDicQuery>();
            RepairEffects = new List<SoilRepairDicQuery>();
        }
        public int ID { set; get; }
        public string RepairAgentNo { set; get; }

        public string RepairAgentSecondNo { set; get; }

        /// <summary>
        /// 处理时间
        /// </summary>
        public decimal? HandleTime { set; get; }

        public string HandleTimeStr { set; get; }

        /// <summary>
        /// 固液比
        /// </summary>
        public string GYratio { set; get; }


        public decimal? PH { set; get; }

        /// <summary>
        /// 电压
        /// </summary>
        public decimal? Voltage { set; get; }

        public int? RepairTechID { set; get; }

        public string RepairTech { set; get; }

        public string RepairName { set; get; }

        /// <summary>
        /// 研发时间
        /// </summary>
        public DateTime? ResearchTime { set; get; }

        public string ResearchTimeStr { set; get; }

        public int ResearchYear { set; get; }

        /// <summary>
        /// 资源类型
        /// </summary>
        public string ResourceType { set; get; }



        public string CreateUser { set; get; }

        public string Researcher { set; get; }

        public string CreateUserName { set; get; }

        public DateTime CreateTime { set; get; }

        public List<FileQuery> RepairFiles { set; get; }

        public List<SoilRepairAssProject> RepairProjects { set; get; }

        /// <summary>
        /// 修复剂组分
        /// </summary>
        public List<SoilRepairDicQuery> RepairComNames { set; get; }

        /// <summary>
        /// 修复效率
        /// </summary>
        public List<SoilRepairDicQuery> RepairEffects { set; get; }

    }

    public class FileQuery
    {
        public int ID { set; get; }

        public string FileName { set; get; }

        public string FilePath { set; get; }

        /// <summary>
        /// 1：修复剂文件  2.工程文件
        /// </summary>
        public int FileType { set; get; }
    }

    public class SoilRepairDicQuery
    {
        public int ID { set; get; }

        public int DicID { set; get; }

        public string DicName { set; get; }

        public string Unit { set; get; }

        /// <summary>
        /// 1：数字 2：字符
        /// </summary>
        public int DataType { set; get; }

        public decimal? Value { set; get; }

        public string DataValue { set; get; }
    }

    public class SoilRepairAssProject
    {
        public int ID { set; get; }

        public string Project { set; get; }
    }


    public class SoilRepairQueryCount
    {
        public SoilRepairQueryCount()
        {
            //ResourceTypeCount = new QueryResourceCount();
            ResourceTypeCount = new List<QueryCountModel>();
            RepairTechCount = new List<QueryCountModel>();
            ProvinceCount = new List<QueryCountModel>();
            WriterCount = new List<QueryCountModel>();
            YearCount = new List<QueryCountModel>();
        }
        public List<QueryCountModel> ResourceTypeCount { set; get; }
        public List<QueryCountModel> RepairTechCount { set; get; }

        public List<QueryCountModel> ProvinceCount { set; get; }

        public List<QueryCountModel> WriterCount { set; get; }

        public List<QueryCountModel> YearCount { set; get; }
    }

    public class QueryCountModel
    {
        public int ID { set; get; }

        public int CountType { set; get; }

        public string Name { set; get; }

        public int Count { set; get; }

        public string CreateName { set; get; }
    }



    public class QueryResourceCount
    {
        /// <summary>
        /// 科技论文
        /// </summary>
        public int KJLW_count { set; get; }

        /// <summary>
        /// 中外专利
        /// </summary>
        public int ZWZL_count { set; get; }

        /// <summary>
        /// 实验报告
        /// </summary>
        public int SYBG_count { set; get; }

        /// <summary>
        /// 中试报告
        /// </summary>
        public int ZSBG_count { set; get; }

        /// <summary>
        /// 工程总结
        /// </summary>
        public int GCZJ_count { set; get; }

        /// <summary>
        /// 法律法规
        /// </summary>
        public int FLFG_count { set; get; }

        /// <summary>
        /// 中外标准
        /// </summary>
        public int ZWBZ_count { set; get; }

    }

}
