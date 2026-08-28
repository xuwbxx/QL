namespace Model.Tech.Tech
{
    public class TechVerifyFlow
    {
        public int ID { set; get; }

        public string Software { set; get; }

        public string VerifyTime { set; get; }

        public int FlowStatus { set; get; }

        public string FlowStatusName { set; get; }

        public string UseStartTime { set; get; }

        public string UseEndTime { set; get; }

        public string ApplyPerson { set; get; }

        public string ApplyProject { set; get; }

        public string BackComment { set; get; }
    }
}
