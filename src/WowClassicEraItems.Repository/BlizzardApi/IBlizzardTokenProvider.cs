namespace WowClassicEraItems.Repository.BlizzardApi;

public interface IBlizzardTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
