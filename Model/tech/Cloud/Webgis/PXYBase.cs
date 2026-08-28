namespace Model.Tech.Cloud.Webgis
{
    public class PXYRet<T>
    {
        public int status { set; get; }

        public string time { set; get; }

        public T data { set; get; }
    }

    public class PXYRetTrack<T>
    {
        public int status { set; get; }

        public T points { set; get; }
    }

    public class PXYRetAnchore<T>
    {
        public int status { set; get; }

        public T records { set; get; }
    }
}
