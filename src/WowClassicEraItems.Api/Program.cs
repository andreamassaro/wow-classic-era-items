using WowClassicEraItems.Api.Endpoints.Items;
using WowClassicEraItems.Api.Middleware;
using WowClassicEraItems.Repository.Config;
using WowClassicEraItems.Services.Config;

var builder = WebApplication.CreateBuilder(args);

// Cross-cutting, application-wide setup lives here.
builder.Services.AddOpenApi();

// Feature registrations delegated to their own project's Ioc extension.
builder.Services.AddRepository(builder.Configuration);
builder.Services.AddServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "WowClassicEraItems API"));
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.MapItemEndpoints();

app.Run();

public partial class Program;
