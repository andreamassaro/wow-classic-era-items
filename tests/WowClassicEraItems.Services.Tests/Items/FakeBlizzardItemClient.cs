using WowClassicEraItems.Repository.BlizzardApi;
using WowClassicEraItems.Repository.Items;

namespace WowClassicEraItems.Services.Tests.Items;

/// <summary>Programmable fake so LoadItemsAsync's pagination logic can be tested without hitting the real Blizzard API.</summary>
public class FakeBlizzardItemClient(IReadOnlyList<BlizzardItemPage> pages) : IBlizzardItemClient
{
    private int _callCount;

    public int CallCount => _callCount;

    public Task<BlizzardItemPage> GetItemsPageAsync(int minId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = _callCount < pages.Count
            ? pages[_callCount]
            : new BlizzardItemPage(page, page, []);

        _callCount++;

        return Task.FromResult(result);
    }
}
