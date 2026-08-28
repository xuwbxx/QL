namespace Model.Tech.Cloud
{
    public class CloudBaseInfoRequest
    {
        public CloudBaseInfoRequest()
        {
            sysType = "ShjTechCloudSys";
            verifykey = "b179298ae08e1a1c0253f6ef90597c2f";
        }

        public string User4ACode { set; get; }

        public string UserName { set; get; }

        public string OID { set; get; }

        public string sysType { set; get; }

        public string verifykey { set; get; }

    }
}
