namespace Model.Tech.Cloud
{
    /// <summary>
    /// CloudCenter SSO 登录返回的用户信息
    /// </summary>
    public class CloudCenterSSOUserInfo
    {
        public string? UserID { set; get; }
        public string? UserName { set; get; }
        public int TaskID { set; get; }
    }
}
