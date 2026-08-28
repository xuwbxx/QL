namespace Model.Tech.Environment
{
    public class RepairDic
    {
        public RepairDic()
        {
            MetalCons = new List<DictionaryResponse>();
            SoilPros = new List<DictionaryResponse>();
            RepairNames = new List<DictionaryResponse>();
            RepairDicNames = new List<DictionaryResponse>();
            RepairEffs = new List<DictionaryResponse>();
            RepairTechs = new List<DictionaryResponse>();
            Provinces = new List<DictionaryResponse>();
            Writers = new List<DictionaryResponse>();
            Years = new List<DictionaryResponse>();
        }

        /// <summary>
        /// 重金属浓度
        /// </summary>
        public List<DictionaryResponse> MetalCons { set; get; }

        /// <summary>
        /// 土壤特性
        /// </summary>
        public List<DictionaryResponse> SoilPros { set; get; }

        /// <summary>
        /// 修复组分
        /// </summary>
        public List<DictionaryResponse> RepairNames { set; get; }

        /// <summary>
        /// 修复组分名称
        /// </summary>
        public List<DictionaryResponse> RepairDicNames { set; get; }

        /// <summary>
        /// 修复效率
        /// </summary>
        public List<DictionaryResponse> RepairEffs { set; get; }

        /// <summary>
        /// 修复技术
        /// </summary>
        public List<DictionaryResponse> RepairTechs { set; get; }

        public List<DictionaryResponse> Provinces { set; get; }

        public List<DictionaryResponse> Writers { set; get; }

        public List<DictionaryResponse> Years { set; get; }
    }


}
