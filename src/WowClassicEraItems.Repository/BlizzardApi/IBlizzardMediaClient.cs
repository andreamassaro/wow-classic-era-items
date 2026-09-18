namespace WowClassicEraItems.Repository.BlizzardApi;

public interface IBlizzardMediaClient
{
    /// <summary>Fetches the icon URL for an item, or null if it has no "icon" asset.</summary>
    Task<string?> GetItemIconUrlAsync(int blizzardItemId, CancellationToken cancellationToken = default);
}
