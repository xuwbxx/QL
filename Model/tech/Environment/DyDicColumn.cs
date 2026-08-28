namespace Model.Tech.Environment
{
    /// <summary>
    /// 动态字典列(数字型)
    /// </summary>
    public class DyDicColumn
    {
        public int DicID { set; get; }

        public string DicName { set; get; }

        public int Type { set; get; }

        public int DataType { set; get; }

        public string Unit { set; get; }

        public decimal? Value { set; get; }

    }

    /// <summary>
    /// 字符串
    /// </summary>
    public class DyDicStrColumn
    {
        public int DicID { set; get; }

        public string DicName { set; get; }

        public int Type { set; get; }

        public string Unit { set; get; }

        public string Value { set; get; }

    }

    public class DyDicColumnExtra
    {
        public int DicID { set; get; }

        public string DicName { set; get; }

        public int Type { set; get; }

        public string Unit { set; get; }

        public decimal? Value { set; get; }


        public int DicExID { set; get; }
        public string DicExName { set; get; }

        public int TypeEx { set; get; }

        public string UnitEx { set; get; }

        public decimal? ValueEx { set; get; }
    }


}
