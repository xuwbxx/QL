namespace Model.Tech.Cloud.BackManage
{
    public class CloudWindManageTaskInfo
    {
        public int ID { set; get; }

        public string ProjectCode { set; get; }

        public string ProjectName { set; get; }

        public int TaskID { set; get; }

        public string TaskCode { set; get; }

        public string TaskName { set; get; }

        public bool IsTimeOut { set; get; }

        public string SendStartTime { set; get; }

        public string SendEndTime { set; get; }
    }
}
