namespace DataFactory.Factory
{

    public interface ITableBase<T>
    {
        T ID { get; set; }
        /// <summary>
        /// 状态：0-保存，1-提交，-1-删除
        /// </summary>
        int Status { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        string? CreatedBy { set; get; }

        /// <summary>
        /// 创建时间（业务时区时间）
        /// </summary>
        DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间（业务时区时间）
        /// </summary>
        DateTime? UpdatedTime { set; get; }
    }
    public interface IIDTable : ITableBase<int>
    {
    }
    public interface ILongIDTable : ITableBase<long>
    {
    }
}
