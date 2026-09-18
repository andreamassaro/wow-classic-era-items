namespace WowClassicEraItems.Repository.Items;

/// <summary>
/// A single item's raw payload ready to be persisted: the Blizzard item id and
/// the raw JSON "data" node fetched from the Blizzard API.
/// </summary>
public record ItemRawData(int BlizzardItemId, string RawJson);
