namespace Model.Tech.Cloud.BackManage
{
    public class CloudWindManageProjectRole
    {
        public int ProjectID { set; get; }

        public string ProjectName { set; get; }
        public CloudWindManageProjectRole()
        {
            List = new List<CloudWindManageProjectRoleData>();
            Roles = new List<CloudWindManageRole>();
        }
        public List<CloudWindManageProjectRoleData> List { set; get; }

        public List<CloudWindManageRole> Roles { set; get; }
    }

    public class CloudWindManageProjectRoleData
    {
        public int ID { set; get; }

        public int RoleID { set; get; }

        public string RoleName { set; get; }

        public string UserName { set; get; }

        public string UserCode { set; get; }

        public string UserDepartName { set; get; }

        public string UserPhone { set; get; }

        public string UserJobName { set; get; }

    }

    public class CloudWindManageRole
    {
        public int ID { set; get; }

        public string RoleName { set; get; }
    }


    public class CloudWindManageTaskDeliver
    {
        public CloudWindManageTaskDeliver()
        {
            Delivers = new List<CloudWindManageProjectRoleData>();
        }

        public string ProjectCode { set; get; }

        public string ProjectName { set; get; }
        public int TaskID { set; get; }

        public string TaskName { set; get; }

        public List<CloudWindManageProjectRoleData> Delivers { set; get; }
    }

}
