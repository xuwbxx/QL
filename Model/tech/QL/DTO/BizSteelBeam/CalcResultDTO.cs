using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.tech.QL.DTO.BizSteelBeam
{
    public class CalcResultDTO
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjName { get; set; } = string.Empty;
        /// <summary>
        /// 编号
        /// </summary>
        public string SheetNo {  get; set; } = string.Empty;
        /// <summary>
        /// 事由
        /// </summary>
        public string Reason {  get; set; } = string.Empty;
        /// <summary>
        /// 前提资料
        /// </summary>
        public string PreDocument {  get; set; } = string.Empty;
        /// <summary>
        /// 已完工节段
        /// </summary>
        public string MeasuredSegments {  get; set; } = string.Empty;
        /// <summary>
        /// 待施工节段
        /// </summary>
        public string PendingSegments {  get; set; } = string.Empty;
        /// <summary>
        /// 测点位置示意图
        /// </summary>
        public string PointMap {  get; set; } = string.Empty;
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set;  } = string.Empty;
        /// <summary>
        /// 编制人
        /// </summary>
        public string Author {  get; set; } = string.Empty;
        /// <summary>
        /// 签发人
        /// </summary>
        public string IssuedBy {  get; set; } = string.Empty;
        /// <summary>
        /// 签发日期
        /// </summary>
        public DateTime? IssuedDate { get; set; }
        /// <summary>
        /// 签收人
        /// </summary>
        public string ReceivedBy {  get; set; } = string.Empty;
        /// <summary>
        /// 签收日期
        /// </summary>
        public DateTime? ReceivedDate { get; set;  }
        /// <summary>
        /// 待拼装梁段数据
        /// </summary>
        public List<CalcResultRowDTO> Rows { get; set; } = new();

        public string EnvFolder {  get; set; } = string.Empty;
    }
    public class CalcResultRowDTO
    {
        /// <summary>
        /// 测点编号
        /// </summary>
        public string PointCode { get; set; } = string.Empty;
        /// <summary>
        /// 待拼装X
        /// </summary>
        public decimal DesignX { set; get; }
        /// <summary>
        /// 待拼装Y
        /// </summary>
        public decimal DesignY { set; get; }
        /// <summary>
        /// 待拼装Z
        /// </summary>
        public decimal DesignZ { set; get; }
        /// <summary>
        /// 梁段号
        /// </summary>
        public string SegmentNo { get; set; } = string.Empty;
        /// <summary>
        /// 测位名称
        /// </summary>
        public string PositionName { get; set; } = string.Empty;
    }

    /// <summary>计算页签梁段下拉项：段号 + 是否已拼装。</summary>
    public class SteelBeamCalcSegmentOptionDTO
    {
        /// <summary>梁段号。</summary>
        public string SegmentNo { get; set; } = string.Empty;
        /// <summary>是否已拼装（该段至少有一个实测来源的点）。</summary>
        public bool IsAssembled { get; set; }
    }
}
