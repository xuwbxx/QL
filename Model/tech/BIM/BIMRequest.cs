using Model.Base;

namespace Model.Tech.BIM
{
    public class BIMRequest : BaseRequest
    {
        public int ID { set; get; }

        public string Name { set; get; }

        public string TableName { set; get; }

        public string ProjectName { set; get; }

        public string UserName { set; get; }

        public string ShipName { set; get; }

        public string ImportTableName { set; get; }

        public string ImportShipName { set; get; }

        public string ImportTypeName { set; get; }
    }
}
