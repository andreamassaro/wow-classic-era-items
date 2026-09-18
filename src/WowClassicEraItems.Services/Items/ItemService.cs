using WowClassicEraItems.Domain.Items;
using WowClassicEraItems.Repository.BlizzardApi;
using WowClassicEraItems.Repository.Items;

namespace WowClassicEraItems.Services.Items;

public class ItemService(IItemRepository itemRepository, IBlizzardItemClient blizzardItemClient, IBlizzardMediaClient blizzardMediaClient)
{
    private const int PageSize = 100;

    public Task<IReadOnlyList<Item>> GetAllAsync(CancellationToken cancellationToken = default)
        => itemRepository.GetAllAsync(cancellationToken);

    /// <summary>
    /// Returns the item, resolving and persisting its icon URL from Blizzard's
    /// media endpoint on first access if it hasn't been looked up yet.
    /// </summary>
    public async Task<Item?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await itemRepository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(item.Icon))
        {
            var iconUrl = await blizzardMediaClient.GetItemIconUrlAsync(item.Id, cancellationToken);
            if (!string.IsNullOrEmpty(iconUrl))
            {
                await itemRepository.UpdateIconAsync(item.Id, iconUrl, cancellationToken);
                item.Icon = iconUrl;
            }
        }

        return item;
    }

    /// <summary>
    /// Fetches every item from the Blizzard item search API and stores them.
    /// Blizzard's search endpoint caps each query's result window at 1000 rows,
    /// so once a window is exhausted this advances the id filter past the
    /// highest id seen and keeps going until a window comes back empty.
    /// </summary>
    public async Task<int> LoadItemsAsync(CancellationToken cancellationToken = default)
    {
        var timestampUtc = DateTime.UtcNow;
        var totalImported = 0;
        var minId = 1;

        while (true)
        {
            var page = 1;
            var pageCount = 1;
            var maxIdSeen = 0;
            var windowHadItems = false;

            do
            {
                var result = await blizzardItemClient.GetItemsPageAsync(minId, page, PageSize, cancellationToken);
                pageCount = result.PageCount;

                if (result.Items.Count == 0)
                {
                    break;
                }

                windowHadItems = true;
                await itemRepository.AddRangeAsync(result.Items, timestampUtc, cancellationToken);
                totalImported += result.Items.Count;
                maxIdSeen = Math.Max(maxIdSeen, result.Items.Max(x => x.BlizzardItemId));

                page++;
            } while (page <= pageCount);

            if (!windowHadItems)
            {
                break;
            }

            minId = maxIdSeen + 1;
        }

        return totalImported;
    }
}
