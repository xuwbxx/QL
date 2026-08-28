namespace Model.Tech.Cloud
{
    public class CloudWindNextNode
    {
        public CloudWindNextNode()
        {
            NodeUsers = new List<CloudWindNextNodeManage>();
        }

        public int ID { set; get; }

        public string NodeName { set; get; }

        public List<CloudWindNextNodeManage> NodeUsers { set; get; }
    }

    public class CloudWindNextNodeManage
    {
        public int ID { set; get; }

        public string ManageName { set; get; }

        public string ManageDepart { set; get; }

    }

}
