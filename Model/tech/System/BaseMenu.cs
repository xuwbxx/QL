namespace Model.Tech.System
{
    public class BaseMenu
    {
        public BaseMenu()
        {
            Children = new List<BaseMenu>();
        }
        public int ID { set; get; }

        public int ParentID { set; get; }

        public int Depth { set; get; }

        public string MenuName { set; get; }

        public string Class { set; get; }

        public string Path { set; get; }

        /// <summary>
        /// 1:菜单  2：页面
        /// </summary>
        public int Type { set; get; }

        public List<BaseMenu> Children { set; get; }
    }

    public class BaseMenuTree
    {
        public BaseMenuTree()
        {
            selected = false;
            partsel = false;
            children = new List<BaseMenuTree>();
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
        public List<BaseMenuTree> children { get; set; }
    }
}
