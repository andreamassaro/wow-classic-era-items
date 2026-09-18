using WowClassicEraItems.Domain.Items;

namespace WowClassicEraItems.Domain.Tests.Items;

public class ItemTests
{
    [Fact]
    public void CanBeConstructed_WithIdAndName()
    {
        var item = new Item { Id = 1, Name = "Sulfuras, Hand of Ragnaros" };

        Assert.Equal(1, item.Id);
        Assert.Equal("Sulfuras, Hand of Ragnaros", item.Name);
    }
}
