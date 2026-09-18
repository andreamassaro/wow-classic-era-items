using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WowClassicEraItems.Repository.Config;

namespace WowClassicEraItems.Repository.BlizzardApi;

/// <summary>
/// Fetches and caches an OAuth2 client-credentials access token from Battle.net.
/// Cached in memory for the lifetime of this instance, refreshed shortly before
/// it expires.
/// </summary>
public class BlizzardTokenProvider(HttpClient httpClient, IOptions<BlizzardOptions> options) : IBlizzardTokenProvider
{
    private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromSeconds(60);

    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _expiresAtUtc = DateTimeOffset.MinValue;

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _expiresAtUtc - ExpiryBuffer)
        {
            return _cachedToken;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _expiresAtUtc - ExpiryBuffer)
            {
                return _cachedToken;
            }

            var blizzardOptions = options.Value;

            using var request = new HttpRequestMessage(HttpMethod.Post, "/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials"
                })
            };

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{blizzardOptions.ClientId}:{blizzardOptions.ClientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var accessToken = document.RootElement.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Battle.net token response did not contain an access_token.");
            var expiresInSeconds = document.RootElement.GetProperty("expires_in").GetInt32();

            _cachedToken = accessToken;
            _expiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);

            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }
}
