using WowClassicEraItems.Services.Items;

namespace WowClassicEraItems.Api.Endpoints.Items;

public static class ItemEndpoints
{
    public static IEndpointRouteBuilder MapItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/items").WithTags("Items");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/load", LoadItemsAsync).WithName("LoadItems");

        return app;
    }

    private static async Task<IResult> GetAllAsync(ItemService itemService, CancellationToken cancellationToken)
    {
        var items = await itemService.GetAllAsync(cancellationToken);
        var response = items.Select(ItemResponse.FromItem);

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> GetByIdAsync(int id, ItemService itemService, CancellationToken cancellationToken)
    {
        var item = await itemService.GetByIdAsync(id, cancellationToken);

        return item is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ItemResponse.FromItem(item));
    }

    private static async Task<IResult> LoadItemsAsync(ItemService itemService, CancellationToken cancellationToken)
    {
        var importedCount = await itemService.LoadItemsAsync(cancellationToken);

        return TypedResults.Ok(new LoadItemsResponse(importedCount));
    }
}
