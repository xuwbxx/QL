namespace Model.TechCenter.Monitor
{

    public enum ProjectTypeEnum
    {

        填海工程 = 1,
        基坑工程 = 2,
        环境工程 = 3,
        桥梁工程 = 4

    }

    public class ProjectInfoResponse
    {
        public ProjectInfoResponse()
        {
            NormalProjects = new List<NormalProject>();
            AbnormalProjectPoints = new List<AbnormalProjectPoint>();

            thProject = new ProjectInfo();
            jkProject = new ProjectInfo();
            hjProject = new ProjectInfo();
            qlProject = new ProjectInfo();
        }

        public string ProjectName { set; get; }
        /// <summary>
        /// 填海
        /// </summary>
        public ProjectInfo thProject { set; get; }
        /// <summary>
        /// 基坑
        /// </summary>
        public ProjectInfo jkProject { set; get; }
        /// <summary>
        /// 环境
        /// </summary>
        public ProjectInfo hjProject { set; get; }
        /// <summary>
        /// 桥梁
        /// </summary>
        public ProjectInfo qlProject { set; get; }
        /// <summary>
        /// 监控正常项目列表
        /// </summary>
        public List<NormalProject> NormalProjects { set; get; }
        /// <summary>
        /// 监控异常项目列表
        /// </summary>
        public List<AbnormalProjectPoint> AbnormalProjectPoints { set; get; }

    }


    public class ProjectInfo
    {
        public ProjectInfo()
        {
            YearNormalProjectCountChart = new List<YearNormalProjectChart>();
            Projects = new List<ProjectAlarm>();
        }

        public ProjectTypeEnum ProjectEnum { set; get; }

        public string ProjectName { set; get; }

        public int ProjectCount { set; get; }

        public int ProjectFinishedCount { set; get; }

        public int ProjectUnfinishedCount { set; get; }

        public List<YearNormalProjectChart> YearNormalProjectCountChart { set; get; }

        public int NormalProjectCount { set; get; }

        public int AbnormalProjectCount { set; get; }

        public string ProjectPic { set; get; }

        public string ProjectUrl { set; get; }

        public List<ProjectAlarm> Projects { set; get; }
    }

    /// <summary>
    /// 监控正常项目数量
    /// </summary>
    public class YearNormalProjectChart
    {
        public int ProjectCount { set; get; }

        public int Year { set; get; }

    }

    /// <summary>
    /// 监控正常项目列表
    /// </summary>
    public class NormalProject
    {
        public ProjectTypeEnum ProjectEnum { set; get; }

        public int ProjectID { set; get; }

        public string ProjectName { set; get; }

        public string PointName { set; get; }

        public string Type { set; get; }

        public DateTime CreateTime { set; get; }
    }


    public class AbnormalProjectPoint
    {
        public ProjectTypeEnum ProjectEnum { set; get; }

        public int ProjectID { set; get; }

        public string ProjectName { set; get; }

        public string ProjectPoint { set; get; }

        public DateTime CreateTime { set; get; }

        public string Time { set; get; }
    }

    public class ProjectAlarm
    {
        public ProjectTypeEnum ProjectEnum { set; get; }

        public int ProjectID { set; get; }

        public string ProjectName { set; get; }

        public int AlarmCount { set; get; }

    }
}
