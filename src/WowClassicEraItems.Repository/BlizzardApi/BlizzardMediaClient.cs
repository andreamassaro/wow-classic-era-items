using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WowClassicEraItems.Repository.Config;

namespace WowClassicEraItems.Repository.BlizzardApi;

public class BlizzardMediaClient(HttpClient httpClient, IBlizzardTokenProvider tokenProvider, IOptions<BlizzardOptions> options) : IBlizzardMediaClient
{
    public async Task<string?> GetItemIconUrlAsync(int blizzardItemId, CancellationToken cancellationToken = default)
    {
        var blizzardOptions = options.Value;
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/data/wow/media/item/{blizzardItemId}?namespace={blizzardOptions.Namespace}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("assets", out var assets))
        {
            return null;
        }

        foreach (var asset in assets.EnumerateArray())
        {
            if (asset.TryGetProperty("key", out var key) && key.GetString() == "icon"
                && asset.TryGetProperty("value", out var value))
            {
                return value.GetString();
            }
        }

        return null;
    }
}
