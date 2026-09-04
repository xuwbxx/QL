using System.Collections.Concurrent;

namespace QL.PreAssembled.Middleware.Auth
{
    /// <summary>
    /// 内存实现，仅适合单实例
    /// </summary>
    public class InMemoryRefreshSessionStore : IRefreshSessionStore
    {
        private readonly ConcurrentDictionary<string, RefreshSessionItem> _store = new();

        public void Add(RefreshSessionItem item)
        {
            _store[item.RefreshToken] = item;
        }

        public RefreshSessionItem? GetByRefreshToken(string refreshToken)
        {
            if (_store.TryGetValue(refreshToken, out var item))
            {
                if (item.ExpireTime < DateTime.UtcNow || item.IsRevoked)
                    return null;
                return item;
            }
            return null;
        }

        public void RevokeAllByUserId(long userId)
        {
            foreach (var kv in _store)
            {
                if (kv.Value.UserId == userId)
                    kv.Value.IsRevoked = true;
            }
        }

        public void RevokeToken(string refreshToken)
        {
            if (_store.TryGetValue(refreshToken, out var item))
            {
                item.IsRevoked = true;
            }
        }

        public void CleanExpired()
        {
            var now = DateTime.UtcNow;
            var removeKeys = _store.Where(kv => kv.Value.ExpireTime < now).Select(x => x.Key).ToList();
            foreach (var key in removeKeys)
            {
                _store.TryRemove(key, out _);
            }
        }
    }
}
