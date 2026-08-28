namespace Model.tech.QL.DTO.SysUser
{
    public class SysUserPermissionDTO
    {
        /// <summary>
        /// 
        /// </summary>
        public List<SysUserMenuDTO> Menus { get; set; } = new();
        /// <summary>
        /// 
        /// </summary>
        public List<string> Permissions { get; set; } = new();
    }
}
