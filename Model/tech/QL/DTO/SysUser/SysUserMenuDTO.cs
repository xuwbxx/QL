namespace Model.tech.QL.DTO.SysUser
{
    public class SysUserMenuDTO
    {
        public int ID { get; set; }
        public int? ParentID { get; set; }
        public string Name { get; set; }
        public string EnName { get; set; }
        public string FullName { get; set; }
        public string EnFullName { get; set; }
        public string? Icon { get; set; }
        public string? Action { get; set; }
        public string? MenuType { get; set; }
        public int Sort { get; set; }

        /// <summary>
        /// 子菜单列表（仅树形版本使用）
        /// </summary>
        public List<SysUserMenuDTO> Children { get; set; } = new();
    }
}
