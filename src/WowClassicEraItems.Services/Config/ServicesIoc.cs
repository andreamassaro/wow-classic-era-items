using Microsoft.Extensions.DependencyInjection;
using WowClassicEraItems.Services.Items;

namespace WowClassicEraItems.Services.Config;

public static class ServicesIoc
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ItemService>();

        return services;
    }
}
