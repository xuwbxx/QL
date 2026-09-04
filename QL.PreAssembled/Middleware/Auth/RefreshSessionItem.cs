namespace QL.PreAssembled.Middleware.Auth
{
    /// <summary>
    /// 内存存储refresh会话实体（代替数据库/Redis）
    /// </summary>
    public class RefreshSessionItem
    {
        public long UserId { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpireTime { get; set; }
        public bool IsRevoked { get; set; } = false; // 是否作废
    }
}
