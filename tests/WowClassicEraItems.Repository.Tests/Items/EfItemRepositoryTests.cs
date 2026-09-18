using Microsoft.EntityFrameworkCore;
using WowClassicEraItems.Repository.Items;

namespace WowClassicEraItems.Repository.Tests.Items;

public class EfItemRepositoryTests
{
    // Note: the EF Core InMemory provider doesn't support SQL Server computed
    // columns, so Name/Icon aren't derived from RawJson here like they are
    // against the real database (verified manually against SQL Server).
    // These tests cover the repository's plumbing/mapping, not the computed
    // column SQL itself.
    private static WowClassicEraItemsDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<WowClassicEraItemsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task GetAllAsync_ReturnsMappedItems()
    {
        await using var dbContext = CreateContext();
        dbContext.Items.Add(new ItemRecord { BlizzardItemId = 25, RawJson = "{}" });
        await dbContext.SaveChangesAsync();

        var sut = new EfItemRepository(dbContext);

        var items = await sut.GetAllAsync();

        var item = Assert.Single(items);
        Assert.Equal(25, item.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var dbContext = CreateContext();
        var sut = new EfItemRepository(dbContext);

        var item = await sut.GetByIdAsync(999);

        Assert.Null(item);
    }

    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsItem()
    {
        await using var dbContext = CreateContext();
        dbContext.Items.Add(new ItemRecord { BlizzardItemId = 25, RawJson = "{}" });
        await dbContext.SaveChangesAsync();

        var sut = new EfItemRepository(dbContext);

        var item = await sut.GetByIdAsync(25);

        Assert.NotNull(item);
        Assert.Equal(25, item!.Id);
    }

    [Fact]
    public async Task AddRangeAsync_InsertsNewRowsStampedWithTimestamp()
    {
        await using var dbContext = CreateContext();
        var sut = new EfItemRepository(dbContext);
        var timestamp = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        await sut.AddRangeAsync(
            [new ItemRawData(25, "{\"id\":25}"), new ItemRawData(35, "{\"id\":35}")],
            timestamp);

        var records = await dbContext.Items.AsNoTracking().ToListAsync();
        Assert.Equal(2, records.Count);
        Assert.All(records, r => Assert.Equal(timestamp, r.Created));
        Assert.All(records, r => Assert.Equal(timestamp, r.Updated));
    }

    [Fact]
    public async Task AddRangeAsync_CalledTwice_AlwaysInserts_NeverUpdates()
    {
        await using var dbContext = CreateContext();
        var sut = new EfItemRepository(dbContext);

        await sut.AddRangeAsync([new ItemRawData(25, "{\"id\":25}")], new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        await sut.AddRangeAsync([new ItemRawData(25, "{\"id\":25}")], new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));

        var records = await dbContext.Items.AsNoTracking().Where(x => x.BlizzardItemId == 25).ToListAsync();
        Assert.Equal(2, records.Count);
    }

    [Fact]
    public async Task GetByIdAsync_MultipleSnapshots_ReturnsLatestOne()
    {
        await using var dbContext = CreateContext();
        dbContext.Items.AddRange(
            new ItemRecord { BlizzardItemId = 25, RawJson = "{}", Updated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ItemRecord { BlizzardItemId = 25, RawJson = "{}", Updated = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) });
        await dbContext.SaveChangesAsync();
        var latestId = await dbContext.Items.AsNoTracking().Where(x => x.BlizzardItemId == 25).MaxAsync(x => x.Id);

        var sut = new EfItemRepository(dbContext);
        var item = await sut.GetByIdAsync(25);

        Assert.NotNull(item);
        var latestRecord = await dbContext.Items.AsNoTracking().SingleAsync(x => x.Id == latestId);
        Assert.Equal(latestRecord.BlizzardItemId, item!.Id);
    }

    [Fact]
    public async Task GetAllAsync_MultipleSnapshotsOfSameItem_ReturnsOnlyOne()
    {
        await using var dbContext = CreateContext();
        dbContext.Items.AddRange(
            new ItemRecord { BlizzardItemId = 25, RawJson = "{}" },
            new ItemRecord { BlizzardItemId = 25, RawJson = "{}" });
        await dbContext.SaveChangesAsync();

        var sut = new EfItemRepository(dbContext);
        var items = await sut.GetAllAsync();

        Assert.Single(items);
    }

    [Fact]
    public async Task UpdateIconAsync_PersistsIconOnLatestSnapshot()
    {
        await using var dbContext = CreateContext();
        dbContext.Items.AddRange(
            new ItemRecord { BlizzardItemId = 25, RawJson = "{}", Updated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ItemRecord { BlizzardItemId = 25, RawJson = "{}", Updated = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) });
        await dbContext.SaveChangesAsync();
        var sut = new EfItemRepository(dbContext);

        await sut.UpdateIconAsync(25, "https://example.com/icon.jpg");

        var item = await sut.GetByIdAsync(25);
        Assert.Equal("https://example.com/icon.jpg", item!.Icon);
    }
}
