namespace Model.Tech.WebApi
{
    public class CloudWindResponse
    {

    }

    public class CloudWindTaskFileResponse
    {
        public int ID { set; get; }

        public int TypeID { set; get; }

        public string TypeName { set; get; }

        public string FileName { set; get; }

        public string FileContent { set; get; }

        public string FilePath { set; get; }
    }


}
