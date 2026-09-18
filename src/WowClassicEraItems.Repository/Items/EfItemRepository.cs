using Microsoft.EntityFrameworkCore;
using WowClassicEraItems.Domain.Items;

namespace WowClassicEraItems.Repository.Items;

public class EfItemRepository(WowClassicEraItemsDbContext dbContext) : IItemRepository
{
    public async Task<IReadOnlyList<Item>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await LatestPerBlizzardItemId().ToListAsync(cancellationToken);

        return records.Select(MapToItem).ToList();
    }

    public async Task<Item?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Items
            .AsNoTracking()
            .Where(x => x.BlizzardItemId == id)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return record is null ? null : MapToItem(record);
    }

    public async Task AddRangeAsync(IEnumerable<ItemRawData> items, DateTime timestampUtc, CancellationToken cancellationToken = default)
    {
        var records = items.Select(x => new ItemRecord
        {
            BlizzardItemId = x.BlizzardItemId,
            RawJson = x.RawJson,
            Created = timestampUtc,
            Updated = timestampUtc
        });

        await dbContext.Items.AddRangeAsync(records, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateIconAsync(int blizzardItemId, string iconUrl, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Items
            .Where(x => x.BlizzardItemId == blizzardItemId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (record is null)
        {
            return;
        }

        record.Icon = iconUrl;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// LoadItems always inserts (never updates), so the same BlizzardItemId can
    /// have multiple historical rows. This picks the most recently inserted one
    /// per item.
    /// </summary>
    private IQueryable<ItemRecord> LatestPerBlizzardItemId() =>
        dbContext.Items
            .AsNoTracking()
            .GroupBy(x => x.BlizzardItemId)
            .Select(g => g.OrderByDescending(x => x.Id).First());

    private static Item MapToItem(ItemRecord record) => new()
    {
        Id = record.BlizzardItemId,
        Name = record.Name ?? string.Empty,
        Icon = record.Icon
    };
}
