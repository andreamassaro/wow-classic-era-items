# Build context is the repository root:
#   docker build -f src/WowClassicEraItems.Api/Dockerfile -t wowclassiceraitems-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY WowClassicEraItems.sln .
COPY src/WowClassicEraItems.Api/WowClassicEraItems.Api.csproj src/WowClassicEraItems.Api/
COPY src/WowClassicEraItems.Services/WowClassicEraItems.Services.csproj src/WowClassicEraItems.Services/
COPY src/WowClassicEraItems.Repository/WowClassicEraItems.Repository.csproj src/WowClassicEraItems.Repository/
COPY src/WowClassicEraItems.Domain/WowClassicEraItems.Domain.csproj src/WowClassicEraItems.Domain/
RUN dotnet restore src/WowClassicEraItems.Api/WowClassicEraItems.Api.csproj

COPY src/ src/
RUN dotnet publish src/WowClassicEraItems.Api/WowClassicEraItems.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WowClassicEraItems.Api.dll"]
