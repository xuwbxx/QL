namespace Model.Tech.System
{
    public class SystemRole
    {
        public SystemRole()
        {
            this.trees = new List<SystemMenu>();
        }
        public int ID { set; get; }
        public string RoleName { set; get; }
        public string RoleCode { set; get; }

        public List<SystemMenu> trees { set; get; }
    }
}
