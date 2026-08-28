namespace Model.tech.QL.DTO.BizProject
{
    public class BizProjectQueryDTO
    {
        public string Account { set; get; }
        public string Password { set; get; }

        /// <summary>
        /// 项目名称（模糊查询）
        /// </summary>
        public string? Name { set; get; }

        /// <summary>
        /// 项目状态：0=在建, 1=完工
        /// </summary>
        public int? ProgressStatus { set; get; }
    }
}
