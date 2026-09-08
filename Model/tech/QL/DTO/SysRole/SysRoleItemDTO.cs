namespace Model.tech.QL.DTO.SysRole
{
    public class SysRoleItemDTO
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string RoleType { get; set; } = "custom";
        public int Sort { get; set; }
        public string? Remark { get; set; }
        public int Status { get; set; } = 1;
        public int UserCount { get; set; }
        public DateTime? CreatedTime { get; set; }
    }

    public class SysRoleIdDTO { public int ID { get; set; } }
    public class SysRoleStatusDTO : SysRoleIdDTO { public int Status { get; set; } }
    public class SysRoleMenuDTO : SysRoleIdDTO { public List<int> MenuIDs { get; set; } = new(); }
    public class SysRoleUserDTO : SysRoleIdDTO { public List<int> UserIDs { get; set; } = new(); }
    public class SysRoleMenuItemDTO
    {
        public int ID { get; set; }
        public int? ParentID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? MenuType { get; set; }
        public int Sort { get; set; }
        public bool Selected { get; set; }
    }
    public class SysRoleUserItemDTO
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string? EmpNo { get; set; }
        public int Status { get; set; }
        public bool Selected { get; set; }
    }
}
