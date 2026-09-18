using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WowClassicEraItems.Api.Endpoints.Items;
using WowClassicEraItems.Repository.Items;

namespace WowClassicEraItems.Api.Tests.Endpoints.Items;

public class ItemEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ItemEndpointsTests(WebApplicationFactory<Program> factory)
    {
        // Swap the SQL Server-backed repository for the in-memory one so these
        // tests don't depend on a live database.
        var testFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IItemRepository>();
                services.AddSingleton<IItemRepository, InMemoryItemRepository>();
            }));

        _client = testFactory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithItems()
    {
        var response = await _client.GetAsync("/items");

        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<ItemResponse>>();
        Assert.NotNull(items);
        Assert.NotEmpty(items!);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/items/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
