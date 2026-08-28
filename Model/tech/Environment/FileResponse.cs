using Model.Base;

namespace Model.Tech.Environment
{
    public class FileResponse : BaseOperateRight
    {
        public int ID { set; get; }

        public int Type { set; get; }

        public string TypeName { set; get; }

        public string FileName { set; get; }

        public string FilePath { set; get; }

        public int AssID { set; get; }

        public string CreateUser { set; get; }

        public string CreateTime { set; get; }



    }
}
