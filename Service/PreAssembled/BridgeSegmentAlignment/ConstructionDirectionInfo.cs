namespace Service.PreAssembled.BridgeSegmentAlignment
{
    /// <summary>
    /// 施工方向信息
    /// </summary>
    public class ConstructionDirectionInfo
    {
        public int[] SegmentDirections { get; set; }  // 1=左→右, -1=右→左
        public int[] ChangePoints { get; set; }       // 方向变化的节段索引
        public int DirectionChanges { get; set; }     // 方向变化次数
    }
}
