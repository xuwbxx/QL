namespace QL.PreAssembled.Middleware.Auth
{
    /// <summary>
    /// RefreshToken会话存储契约
    /// </summary>
    public interface IRefreshSessionStore
    {
        void Add(RefreshSessionItem item);

        RefreshSessionItem? GetByRefreshToken(string refreshToken);

        /// <summary>作废该用户全部会话（改密码、踢下线）</summary>
        void RevokeAllByUserId(long userId);

        /// <summary>作废单个refresh token（登出、一次性refresh）</summary>
        void RevokeToken(string refreshToken);

        /// <summary>清理过期会话</summary>
        void CleanExpired();
    }
}
