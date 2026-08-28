namespace Model.Base
{
    /// <summary>
    /// 动态查询
    /// </summary>
    public class QueryCondition
    {
        /// <summary>
        /// 字典ID
        /// </summary>
        public int DicID { set; get; }

        /// <summary>
        /// 1:数字型 2：字符型
        /// </summary>
        public int Type { set; get; }

        public string DicName { set; get; }

        public string MaxValue { set; get; }

        public string MinValue { set; get; }

        public string Value { set; get; }
    }

    public enum DicDataType
    {
        数字型 = 1,
        字符串型 = 2
    }

    public class FilterModel
    {
        public DateTime StartTime { set; get; }

        public DateTime EndTime { set; get; }
    }
}
