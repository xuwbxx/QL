using Microsoft.Extensions.Caching.Memory;

namespace Service.PreAssembled;

public enum ConcreteBeamImportKind
{
    Design,
    Camber
}

public sealed record ConcreteBeamImportPreview<T>(
    string Token,
    string Account,
    int CastingGroupID,
    ConcreteBeamImportKind Kind,
    IReadOnlyList<T> Rows,
    bool ElevationIncludesCamber);

public sealed record ConcreteBeamImportPreviewPage<T>(
    IReadOnlyList<T> List,
    int Total,
    bool ElevationIncludesCamber);

public sealed class ConcreteBeamImportPreviewStore(IMemoryCache cache)
{
    private static readonly TimeSpan PreviewIdleTimeout = TimeSpan.FromMinutes(30);
    private const string CacheKeyPrefix = "concrete-beam-import-preview:";

    public string Create<T>(string account, int castingGroupID, ConcreteBeamImportKind kind,
        IReadOnlyList<T> rows, bool elevationIncludesCamber = false)
    {
        var token = Guid.NewGuid().ToString("N");
        var entry = new StoredPreview(account, castingGroupID, kind, rows, elevationIncludesCamber);
        cache.Set(CacheKeyPrefix + token, entry,
            new MemoryCacheEntryOptions { SlidingExpiration = PreviewIdleTimeout });
        return token;
    }

    public ConcreteBeamImportPreviewPage<T> GetPage<T>(string token, string account,
        int castingGroupID, ConcreteBeamImportKind kind, int pageIndex, int pageSize)
    {
        var preview = GetForConfirm<T>(token, account, castingGroupID, kind);
        var page = Math.Max(pageIndex, 1);
        var size = Math.Max(pageSize, 1);
        var list = preview.Rows.Skip((page - 1) * size).Take(size).ToList();
        return new(list, preview.Rows.Count, preview.ElevationIncludesCamber);
    }

    public ConcreteBeamImportPreview<T> GetForConfirm<T>(string token, string account,
        int castingGroupID, ConcreteBeamImportKind kind)
    {
        var stored = GetStored(token);
        if (!string.Equals(stored.Account, account, StringComparison.Ordinal) ||
            stored.CastingGroupID != castingGroupID || stored.Kind != kind)
            throw new UnauthorizedAccessException("无权访问该导入预览");
        if (stored.Rows is not IReadOnlyList<T> rows)
            throw new InvalidOperationException("导入预览数据类型不匹配，请重新导入");
        return new(token, account, castingGroupID, kind, rows, stored.ElevationIncludesCamber);
    }

    public void Remove(string token, string account, int castingGroupID, ConcreteBeamImportKind kind)
    {
        _ = GetForConfirm<object>(token, account, castingGroupID, kind);
        cache.Remove(CacheKeyPrefix + token);
    }

    public void RemoveConfirmed(string token) => cache.Remove(CacheKeyPrefix + token);

    private StoredPreview GetStored(string token)
    {
        if (string.IsNullOrWhiteSpace(token) ||
            !cache.TryGetValue(CacheKeyPrefix + token, out StoredPreview? stored) || stored == null)
            throw new InvalidOperationException("导入预览已失效，请重新导入");
        return stored;
    }

    private sealed record StoredPreview(
        string Account,
        int CastingGroupID,
        ConcreteBeamImportKind Kind,
        object Rows,
        bool ElevationIncludesCamber);
}
