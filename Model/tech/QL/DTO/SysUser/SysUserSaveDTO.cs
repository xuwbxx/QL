using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.tech.QL.DTO.SysUser
{
    /// <summary>
    /// 用户保存DTO（新增/编辑）
    /// </summary>
    public class SysUserSaveDTO
    {
        public int ID { get; set; }

        /// <summary>
        /// 登录账号（即交建通userLogin，作为登录账号）
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 员工编号（只读）
        /// </summary>
        public string? EmpNo { get; set; }

        /// <summary>
        /// 登录密码（MD5）
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 确认密码（仅前端校验）
        /// </summary>
        public string? ConfirmPassword { get; set; }

        /// <summary>
        /// 姓名（只读）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 角色ID列表
        /// </summary>
        public List<int> RoleIds { get; set; } = new List<int>();

        /// <summary>
        /// 电话
        /// </summary>
        public string? Mobile { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 部门OID
        /// </summary>
        public string? DeptOID { get; set; }

        /// <summary>
        /// 部门名称（完整部门路径）
        /// </summary>
        public string? DeptName { get; set; }
    }
}
