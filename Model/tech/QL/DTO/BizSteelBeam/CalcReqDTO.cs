using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.tech.QL.DTO.BizSteelBeam
{
    public class CalcReqDTO
    {
        /// <summary>
        /// 梁ID
        /// </summary>
        public int GirderID { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjName { get; set; } = string.Empty;
        /// <summary>
        /// 编号
        /// </summary>
        public string SheetNo { get; set; } = string.Empty;
        /// <summary>
        /// 事由
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        /// <summary>
        /// 前提资料
        /// </summary>
        public string PreDocument { get; set; } = string.Empty;
        /// <summary>
        /// 已完工节段
        /// </summary>
        public string MeasuredSegments { get; set; } = string.Empty;
        /// <summary>
        /// 待施工节段
        /// </summary>
        public string PendingSegments { get; set; } = string.Empty;
        /// <summary>
        /// 测点位置示意图
        /// </summary>
        public string PointMap { get; set; } = string.Empty;
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// 编制人
        /// </summary>
        public string Author { get; set; } = string.Empty;
        /// <summary>
        /// 签发人
        /// </summary>
        public string IssuedBy { get; set; } = string.Empty;
        /// <summary>
        /// 签发日期
        /// </summary>
        public DateTime? IssuedDate { get; set; }
        /// <summary>
        /// 签收人
        /// </summary>
        public string ReceivedBy { get; set; } = string.Empty;
        /// <summary>
        /// 运行环境目录
        /// </summary>
        public string EnvFolder { get; set; }
        /// <summary>
        /// 签收日期
        /// </summary>
        public DateTime? ReceivedDate { get; set; }

    }
}
