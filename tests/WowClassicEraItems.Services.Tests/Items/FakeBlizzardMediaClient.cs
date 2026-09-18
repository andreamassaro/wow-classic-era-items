using WowClassicEraItems.Repository.BlizzardApi;

namespace WowClassicEraItems.Services.Tests.Items;

/// <summary>Programmable fake so icon-resolution logic can be tested without hitting the real Blizzard API.</summary>
public class FakeBlizzardMediaClient(string? iconUrl = null) : IBlizzardMediaClient
{
    public int CallCount { get; private set; }

    public Task<string?> GetItemIconUrlAsync(int blizzardItemId, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult(iconUrl);
    }
}
