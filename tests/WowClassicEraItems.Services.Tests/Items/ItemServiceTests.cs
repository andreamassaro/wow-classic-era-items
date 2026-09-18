using WowClassicEraItems.Repository.BlizzardApi;
using WowClassicEraItems.Repository.Items;
using WowClassicEraItems.Services.Items;

namespace WowClassicEraItems.Services.Tests.Items;

public class ItemServiceTests
{
    private static FakeBlizzardItemClient EmptyBlizzardClient() => new([new BlizzardItemPage(1, 1, [])]);

    [Fact]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        var repository = new InMemoryItemRepository();
        var sut = new ItemService(repository, EmptyBlizzardClient(), new FakeBlizzardMediaClient());

        var items = await sut.GetAllAsync();

        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMatchingItem()
    {
        var repository = new InMemoryItemRepository();
        var sut = new ItemService(repository, EmptyBlizzardClient(), new FakeBlizzardMediaClient());
        var expected = (await repository.GetAllAsync()).First();

        var item = await sut.GetByIdAsync(expected.Id);

        Assert.NotNull(item);
        Assert.Equal(expected.Name, item!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_MissingIcon_ResolvesAndPersistsFromMediaClient()
    {
        var repository = new InMemoryItemRepository();
        var mediaClient = new FakeBlizzardMediaClient("https://render.worldofwarcraft.com/classic1x-eu/icons/56/inv_sword_04.jpg");
        var sut = new ItemService(repository, EmptyBlizzardClient(), mediaClient);
        var expected = (await repository.GetAllAsync()).First();

        var item = await sut.GetByIdAsync(expected.Id);

        Assert.NotNull(item);
        Assert.Equal("https://render.worldofwarcraft.com/classic1x-eu/icons/56/inv_sword_04.jpg", item!.Icon);
        Assert.Equal(1, mediaClient.CallCount);

        // Second call shouldn't need to resolve again, since it's now persisted.
        await sut.GetByIdAsync(expected.Id);
        Assert.Equal(1, mediaClient.CallCount);
    }

    [Fact]
    public async Task LoadItemsAsync_PagesThroughResultsAndStopsOnEmptyWindow()
    {
        var repository = new InMemoryItemRepository();
        var blizzardClient = new FakeBlizzardItemClient([
            new BlizzardItemPage(1, 2, [new ItemRawData(101, "{\"id\":101,\"name\":{\"en_US\":\"A\"}}"), new ItemRawData(102, "{\"id\":102,\"name\":{\"en_US\":\"B\"}}")]),
            new BlizzardItemPage(2, 2, [new ItemRawData(103, "{\"id\":103,\"name\":{\"en_US\":\"C\"}}")]),
            new BlizzardItemPage(1, 1, [])
        ]);
        var sut = new ItemService(repository, blizzardClient, new FakeBlizzardMediaClient());
        var initialCount = (await repository.GetAllAsync()).Count;

        var imported = await sut.LoadItemsAsync();

        Assert.Equal(3, imported);
        Assert.Equal(3, blizzardClient.CallCount);
        var allItems = await repository.GetAllAsync();
        Assert.Equal(initialCount + 3, allItems.Count);
    }
}
