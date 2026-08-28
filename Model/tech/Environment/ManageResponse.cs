namespace Model.Tech.Environment
{
    public class ManageResponse
    {

    }

    public class UserManageResponse
    {
        public int ID { set; get; }

        public string Name { set; get; }

        public string UserName { set; get; }

        public string Password { set; get; }

        public string UserCode { set; get; }

        public string Depart { set; get; }

        public string Phone { set; get; }

        public int RoleID { set; get; }
        public string Role { set; get; }

        public int DataRightID { set; get; }
        public string DataRight { set; get; }

        public int ViewRightID { set; get; }
        public string ViewRight { set; get; }


        public string CreateUser { set; get; }

        public string CreateTime { set; get; }
    }
}
