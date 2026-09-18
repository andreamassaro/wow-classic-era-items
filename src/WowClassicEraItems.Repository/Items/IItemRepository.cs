using WowClassicEraItems.Domain.Items;

namespace WowClassicEraItems.Repository.Items;

public interface IItemRepository
{
    /// <summary>Returns the latest snapshot of every item.</summary>
    Task<IReadOnlyList<Item>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the latest snapshot of the item with the given Blizzard item id.</summary>
    Task<Item?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a batch of raw item payloads, always as new rows (never updates
    /// existing ones), stamped with <paramref name="timestampUtc"/> for both
    /// Created and Updated.
    /// </summary>
    Task AddRangeAsync(IEnumerable<ItemRawData> items, DateTime timestampUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a resolved icon URL onto the latest snapshot of the given item,
    /// so it doesn't need to be looked up again on subsequent reads.
    /// </summary>
    Task UpdateIconAsync(int blizzardItemId, string iconUrl, CancellationToken cancellationToken = default);
}
