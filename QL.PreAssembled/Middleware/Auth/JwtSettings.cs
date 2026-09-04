namespace QL.PreAssembled.Middleware.Auth
{
    // 配置类
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        /// <summary>access过期秒数，例如1800=30分钟</summary>
        public int AccessExpireSeconds { get; set; } = 1800;
        /// <summary>refresh过期秒数，例如7天</summary>
        public int RefreshExpireSeconds { get; set; } = 7 * 24 * 3600;
    }
}
