using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.tech.QL.DTO.SysUser
{
    /// <summary>
    /// 用户列表DTO
    /// </summary>
    public class SysUserItemDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string? EmpNo { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? RoleNames { get; set; }
        public string? DeptName { get; set; }
        public int Status { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
