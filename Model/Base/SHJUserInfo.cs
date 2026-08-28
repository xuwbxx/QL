namespace Model.Base
{
    public class SHJUserInfo
    {
        public int ID { set; get; }

        public int UserID { set; get; }

        public string? UserCode { set; get; }

        public string? UserName { set; get; }

        public string? RealName { set; get; }

        public string? Password { set; get; }

        public string? RoleName { set; get; }

        public int RoleID { set; get; }

        public string? Depart { set; get; }

        public string? Job { set; get; }

        public string? Email { set; get; }

        public string? Mobile { set; get; }

        public string? Birthday { set; get; }

        public int SoftwareID { set; get; }

        public string? DepartName { set; get; }

        public string? JobName { set; get; }
    }

    public class SSOUserInfo
    {

        public string? UserCode { set; get; }

        public string? UserName { set; get; }

        public string? Phone { set; get; }

        public string? Depart { set; get; }
    }
}
