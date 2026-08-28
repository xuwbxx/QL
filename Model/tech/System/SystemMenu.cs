namespace Model.Tech.System
{
    public class SystemMenu
    {
        public SystemMenu()
        {
            selected = false;
            partsel = false;
            children = new List<SystemMenu>();
        }
        public string key { get; set; }
        public string title { get; set; }
        public bool selected { get; set; }
        public bool partsel { get; set; }
        public object data { get; set; }
        public int parentID { get; set; }

        public string Path { set; get; }

        public string FunctionCode { set; get; }
        public int Order { set; get; }

        public string Class { set; get; }
        public List<SystemMenu> children { get; set; }
    }
}
