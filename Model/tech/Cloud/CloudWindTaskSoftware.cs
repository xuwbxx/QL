namespace Model.Tech.Cloud
{
    public class CloudWindTaskSoftware
    {
        public int ID { set; get; }

        public int TaskID { set; get; }

        public string TaskCode { set; get; }

        public string TaskTimeString { set; get; }

        public bool IsUsed { set; get; }
    }
}
