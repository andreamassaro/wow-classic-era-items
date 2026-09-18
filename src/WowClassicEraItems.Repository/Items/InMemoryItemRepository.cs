using System.Text.Json;
using WowClassicEraItems.Domain.Items;

namespace WowClassicEraItems.Repository.Items;

/// <summary>
/// Temporary in-memory implementation. Replace with a real data store
/// (e.g. EF Core DbContext) when persistence is introduced.
/// </summary>
public class InMemoryItemRepository : IItemRepository
{
    private readonly List<Item> _items =
    [
        new Item { Id = 1, Name = "Thunderfury, Blessed Blade of the Windseeker" },
        new Item { Id = 2, Name = "Sulfuras, Hand of Ragnaros" }
    ];

    public Task<IReadOnlyList<Item>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Item>>(_items);
    }

    public Task<Item?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        return Task.FromResult(item);
    }

    public Task AddRangeAsync(IEnumerable<ItemRawData> items, DateTime timestampUtc, CancellationToken cancellationToken = default)
    {
        foreach (var payload in items)
        {
            using var document = JsonDocument.Parse(payload.RawJson);
            var name = document.RootElement.TryGetProperty("name", out var nameElement)
                && nameElement.TryGetProperty("en_US", out var enUsElement)
                    ? enUsElement.GetString() ?? string.Empty
                    : string.Empty;

            _items.Add(new Item { Id = payload.BlizzardItemId, Name = name });
        }

        return Task.CompletedTask;
    }

    public Task UpdateIconAsync(int blizzardItemId, string iconUrl, CancellationToken cancellationToken = default)
    {
        var item = _items.FirstOrDefault(i => i.Id == blizzardItemId);
        if (item is not null)
        {
            item.Icon = iconUrl;
        }

        return Task.CompletedTask;
    }
}
