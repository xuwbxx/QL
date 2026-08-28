namespace Model.Tech.Environment
{
    public class SoilPolluteCompareModel
    {
        public SoilPolluteCompareModel()
        {
            NumberValues = new List<BaseNumValueModel>();
            StringValues = new List<BaseStrValueModel>();
            DicNumberValues = new List<BaseDicNumValueModel>();
        }
        public int ID { set; get; }

        public string Content { set; get; }

        public List<BaseNumValueModel> NumberValues { set; get; }

        public List<BaseStrValueModel> StringValues { set; get; }

        public List<BaseDicNumValueModel> DicNumberValues { set; get; }

    }

    public class BaseNumValueModel
    {
        public int? DicID { set; get; }

        public decimal? Value { set; get; }
    }

    public class BaseDicNumValueModel
    {
        public int? ColumnDicID { set; get; }

        public int? DicID { set; get; }

        public decimal? Value { set; get; }
    }

    public class BaseStrValueModel
    {
        public int? DicID { set; get; }

        public string Value { set; get; }
    }
}
