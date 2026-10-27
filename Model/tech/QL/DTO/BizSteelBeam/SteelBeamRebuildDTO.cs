using System.Collections.Generic;

namespace Model.tech.QL.DTO.BizSteelBeam
{
    /// <summary>
    /// 批量重建推算坐标请求：<see cref="ProjID"/> 与 <see cref="GirderIDs"/> 至少给一个。
    /// </summary>
    public class SteelBeamRebuildBatchDTO
    {
        /// <summary>项目ID；传了就重建该项目下所有钢梁。</summary>
        public int? ProjID { get; set; }

        /// <summary>指定梁ID列表；与 ProjID 同时传时取交集。</summary>
        public List<int>? GirderIDs { get; set; }
    }

    /// <summary>单根钢梁的批量重建结果。</summary>
    public class SteelBeamRebuildResultDTO
    {
        /// <summary>梁ID。</summary>
        public int GirderID { get; set; }

        /// <summary>梁名称。</summary>
        public string GirderName { get; set; } = string.Empty;

        /// <summary>是否成功。</summary>
        public bool Success { get; set; }

        /// <summary>成功时的重算描述，或失败原因。</summary>
        public string Message { get; set; } = string.Empty;
    }
}
