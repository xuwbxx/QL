namespace Model.tech.QL.DTO.SysUser
{
    public class SysUserQueryDTO : EPApiRequest
    {
        public string Name { get; set; }
        public string EmpNo { get; set; }
        public string Mobile { get; set; }
        public string DeptName { set; get; }
        public string Account { set; get; }
    }
}
