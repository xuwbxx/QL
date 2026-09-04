namespace QL.PreAssembled.Middleware.Auth
{
    // 登录返回给前端的模型
    public class LoginResultDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public long ExpiresIn { get; set; } // access有效期，秒，例如1800
    }
}
