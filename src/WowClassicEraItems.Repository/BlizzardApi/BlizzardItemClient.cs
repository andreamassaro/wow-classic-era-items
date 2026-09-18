using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WowClassicEraItems.Repository.Config;
using WowClassicEraItems.Repository.Items;

namespace WowClassicEraItems.Repository.BlizzardApi;

public class BlizzardItemClient(HttpClient httpClient, IBlizzardTokenProvider tokenProvider, IOptions<BlizzardOptions> options) : IBlizzardItemClient
{
    public async Task<BlizzardItemPage> GetItemsPageAsync(int minId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var blizzardOptions = options.Value;
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken);

        var idFilter = Uri.EscapeDataString($"[{minId},]");
        var requestUri = $"/data/wow/search/item?namespace={blizzardOptions.Namespace}&orderby=id&_pageSize={pageSize}&_page={page}&id={idFilter}";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var root = document.RootElement;
        var pageCount = root.GetProperty("pageCount").GetInt32();

        var items = new List<ItemRawData>();
        if (root.TryGetProperty("results", out var results))
        {
            foreach (var result in results.EnumerateArray())
            {
                var data = result.GetProperty("data");
                var blizzardItemId = data.GetProperty("id").GetInt32();
                items.Add(new ItemRawData(blizzardItemId, data.GetRawText()));
            }
        }

        return new BlizzardItemPage(page, pageCount, items);
    }
}
