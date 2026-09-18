using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WowClassicEraItems.Repository.BlizzardApi;
using WowClassicEraItems.Repository.Items;

namespace WowClassicEraItems.Repository.Config;

public static class RepositoryIoc
{
    public static IServiceCollection AddRepository(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WowClassicEraItemsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IItemRepository, EfItemRepository>();

        services.Configure<BlizzardOptions>(configuration.GetSection("Blizzard"));

        services.AddHttpClient<IBlizzardTokenProvider, BlizzardTokenProvider>(client =>
            client.BaseAddress = new Uri("https://oauth.battle.net"));

        services.AddHttpClient<IBlizzardItemClient, BlizzardItemClient>(client =>
            client.BaseAddress = new Uri("https://eu.api.blizzard.com"));

        services.AddHttpClient<IBlizzardMediaClient, BlizzardMediaClient>(client =>
            client.BaseAddress = new Uri("https://eu.api.blizzard.com"));

        return services;
    }
}
