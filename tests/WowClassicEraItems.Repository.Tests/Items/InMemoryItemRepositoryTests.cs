using WowClassicEraItems.Repository.Items;

namespace WowClassicEraItems.Repository.Tests.Items;

public class InMemoryItemRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsSeededItems()
    {
        var repository = new InMemoryItemRepository();

        var items = await repository.GetAllAsync();

        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var repository = new InMemoryItemRepository();

        var item = await repository.GetByIdAsync(int.MaxValue);

        Assert.Null(item);
    }
}
