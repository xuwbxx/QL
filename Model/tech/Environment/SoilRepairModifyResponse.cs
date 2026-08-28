namespace Model.Tech.Environment
{
    public class SoilRepairModifyResponse
    {
        public SoilRepairModifyResponse()
        {
            Dics = new List<DyDicColumn>();
            Values = new List<DyDicColumnExtra>();
        }

        public List<DyDicColumn> Dics { set; get; }

        public List<DyDicColumnExtra> Values { set; get; }
    }
}
