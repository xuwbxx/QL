namespace Model.tech.QL.DTO.SysRole
{
    public class SysRoleQueryDTO
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public int? Status { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
