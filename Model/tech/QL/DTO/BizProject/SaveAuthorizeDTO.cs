namespace Model.tech.QL.DTO.BizProject
{
    /// <summary>
    /// 项目授权保存DTO
    /// </summary>
    public class SaveAuthorizeDTO
    {
        public int ProjId { get; set; }
        public List<int> UserIds { get; set; } = new List<int>();
    }
}
