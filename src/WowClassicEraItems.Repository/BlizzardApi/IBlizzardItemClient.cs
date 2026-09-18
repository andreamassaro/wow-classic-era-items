namespace WowClassicEraItems.Repository.BlizzardApi;

public interface IBlizzardItemClient
{
    /// <summary>
    /// Fetches a page of items with Blizzard item id &gt;= <paramref name="minId"/>,
    /// ordered by id. Blizzard's search endpoint caps the total result window at
    /// 1000 rows per query, so callers should advance <paramref name="minId"/> past
    /// the highest id seen once a window is exhausted to keep paging through the
    /// full catalog.
    /// </summary>
    Task<BlizzardItemPage> GetItemsPageAsync(int minId, int page, int pageSize, CancellationToken cancellationToken = default);
}
