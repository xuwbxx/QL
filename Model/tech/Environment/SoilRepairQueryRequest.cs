using Model.Base;

namespace Model.Tech.Environment
{
    public class SoilRepairQueryRequest : BaseRequest
    {
        public SoilRepairQueryRequest()
        {
            DataTypes = new List<string>();
            SoilPros = new List<QueryDicModel>();
            MetalCons = new List<QueryDicModel>();
            RepairNames = new List<QueryDicModel>();
            RepairEffs = new List<QueryDicModel>();
            RepairTechs = new List<QueryDataString>();
            RepairTimes = new List<QueryTime>();
            Researchers = new List<QueryDataString>();
            Year = new QueryTime();

            OrderBy = new QueryOrderBy();

            ChooseParam = new QueryChooseParam();

            Researcher = new QueryResearcher();
        }

        public int ID { set; get; }

        public QueryOrderBy OrderBy { set; get; }

        public List<string> DataTypes { set; get; }

        public List<QueryDicModel> SoilPros { set; get; }
        public List<QueryDicModel> MetalCons { set; get; }
        public List<QueryDicModel> RepairNames { set; get; }
        public List<QueryDicModel> RepairEffs { set; get; }
        public List<QueryDataString> RepairTechs { set; get; }

        public List<QueryTime> RepairTimes { set; get; }

        public List<QueryDataString> Researchers { set; get; }

        public QueryTime Year { set; get; }

        public QueryChooseParam ChooseParam { set; get; }

        public QueryResearcher Researcher { set; get; }

        public string QueryContent { set; get; }
    }

    /// <summary>
    /// 资源类型
    /// </summary>
    public class QueryDataString
    {
        public int ID { set; get; }
        public string Name { set; get; }
    }

    public class QueryDicModel
    {
        public int DicID { set; get; }

        public int DataType { set; get; }

        public string Value { set; get; }

        public decimal Value1 { set; get; }

        public decimal Value2 { set; get; }
    }


    public class QueryTime
    {
        public DateTime StartTime { set; get; }
        public DateTime EndTime { set; get; }

        public int MinDay { set; get; }

        public int MaxDay { set; get; }

        public int StartYear { set; get; }

        public int EndYear { set; get; }
    }


    public class QueryOrderBy
    {
        public int OrderID { set; get; }

        public int OrderDicID { set; get; }
    }

    public class QueryChooseParam
    {
        public QueryChooseParam()
        {
            DataTypes = new List<string>();
            RepairTechs = new List<int>();
            Provinces = new List<int>();
            Writers = new List<string>();
            Years = new List<int>();
        }

        public bool IsChooseQuery { set; get; }

        public List<string> DataTypes { set; get; }

        public List<int> RepairTechs { set; get; }

        public List<int> Provinces { set; get; }
        public List<string> Writers { set; get; }
        public List<int> Years { set; get; }
    }

    public class QueryResearcher
    {
        public int Type { set; get; }

        public string Researcher { set; get; }
    }
}
