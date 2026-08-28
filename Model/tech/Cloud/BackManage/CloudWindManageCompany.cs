namespace Model.Tech.Cloud.BackManage
{
    public class CloudWindManageCompany
    {
        public CloudWindManageCompany()
        {
            //MajorEng = new CloudWindBackManageUserInfo();
        }

        public int ID { set; get; }

        public string Company { set; get; }

        /// <summary>
        /// 总工
        /// </summary>
        public CloudWindBackManageUserInfo MajorEng;
    }


}
