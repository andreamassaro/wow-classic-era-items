using WowClassicEraItems.Domain.Items;

namespace WowClassicEraItems.Api.Endpoints.Items;

public record ItemResponse(int Id, string Name, string? Icon)
{
    public static ItemResponse FromItem(Item item) => new(item.Id, item.Name, item.Icon);
}
