using Model.Base;

namespace Model.Tech.System
{
    public class SystemRequest : BaseRequest
    {
        public int ID { set; get; }

        //账号
        public string Name { set; get; }

        public string UserName { set; get; }

        public string Password { set; get; }

        public int RoleID { set; get; }

        public int DepartID { set; get; }

        public int PositionID { set; get; }

        public int CompanyID { set; get; }

        public string Phone { set; get; }

        public string CompanyIDs { set; get; }

        //菜单
        public string Menu { set; get; }

        public int Order { set; get; }

        public string Path { set; get; }

        public string Class { set; get; }

        public int Depth { set; get; }

        public int ParentID { set; get; }

        public List<int> MenuIDs { set; get; }
    }
}
