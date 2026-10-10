using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.tech.QL.DTO.BizSteelBeam
{
    public class CalcSteelBeamOffsetDTO
    {
        /// <summary>
        /// 偏差类型
        /// </summary>
        public string OffsetType { set; get; } = string.Empty;
        /// <summary>
        /// 偏差数组
        /// </summary>
        public List<CalcSteelBeamOffsetRow> Rows { set; get; } = new();
    }
    public class CalcSteelBeamOffsetRow
    {
        /// <summary>
        /// 偏差值，作为误差分析的Y轴值
        /// </summary>
        public decimal? Value { set; get; }
        /// <summary>
        /// 点位到初始点的距离，作为误差分析的X轴值
        /// </summary>
        public decimal? PointDistance { set; get; }
        /// <summary>
        /// 测点编号
        /// </summary>
        public string PointCode { set; get; } = string.Empty;
        /// <summary>
        /// 所属测位
        /// </summary>
        public string PositionName { set; get; } = string.Empty;
        /// <summary>
        /// 测位内顺序
        /// </summary>
        public int? PositionOrder { set; get; }
        /// <summary>
        /// 顶推次数
        /// </summary>
        public int PushCount { set; get; }
        /// <summary>
        /// 所属梁段
        /// </summary>
        public string SegmentNo { set; get; } = string.Empty;
        /// <summary>
        /// 点位X坐标
        /// </summary>
        public decimal X { set; get; }
        /// <summary>
        /// 点位Y坐标
        /// </summary>
        public decimal Y { set; get; }
        /// <summary>
        /// 点位Z坐标
        /// </summary>
        public decimal Z { set; get; }

        public decimal PredictZ { set; get; }
        public decimal ZError { set; get; }
    }
}
