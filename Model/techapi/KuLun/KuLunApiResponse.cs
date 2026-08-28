namespace Model.TechApi.KuLun
{
    public class KuLunApiResponse
    {

    }

    public class KuLunReturn<T>
    {
        public int errcode { set; get; }

        public string errmsg { set; get; }

        public T data { set; get; }

    }

    public class KuLunProject
    {
        /// <summary>
        /// | id | Integer | 项目ID |
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// | projectName | String | 项目名称 |
        /// </summary>
        public string projectName { set; get; }
    }

    public class KuLunProjectPoint
    {
        /// <summary>
        /// | id | Integer | 测点ID |
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// | pid | Integer | 项目ID |
        /// </summary>
        public int pid { set; get; }

        /// <summary>
        /// | pointNumber | String | 测点编号 |
        /// </summary>
        public string pointNumber { set; get; }

        /// <summary>
        /// | monitoringType | String | 监测类型 |
        /// </summary>
        public string monitoringType { set; get; }
    }

}
