using WowClassicEraItems.Repository.Items;

namespace WowClassicEraItems.Repository.BlizzardApi;

/// <summary>One page of the Blizzard item search results.</summary>
public record BlizzardItemPage(int Page, int PageCount, IReadOnlyList<ItemRawData> Items);
